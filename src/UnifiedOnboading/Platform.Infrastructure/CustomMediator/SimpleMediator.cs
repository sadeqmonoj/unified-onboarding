using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Platform.BuildingBlocks.CustomMediator;

namespace Platform.Infrastructure.CustomMediator;

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

public class SimpleMediator : IMediator
{
    private readonly IServiceProvider _provider;
    // Cache compiled handler invokers: key = "requestType|responseType"
    private static readonly ConcurrentDictionary<string, Func<IServiceProvider, object, CancellationToken, Task<object>>> _handlerInvokerCache
        = new();
    // Cache behavior Handle MethodInfo per runtime behavior type for faster invocation
    private static readonly ConcurrentDictionary<Type, MethodInfo?> _behaviorHandleMethodInfoCache
        = new();
    public SimpleMediator(IServiceProvider provider) => _provider = provider;
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }
        Type requestType = request.GetType();
        Type responseType = typeof(TResponse);
        // Build cache key
        string cacheKey = $"{requestType.FullName}|{responseType.FullName}";
        // Get compiled handler invoker (creates & caches on first use)
        Func<IServiceProvider, object, CancellationToken, Task<object>> handlerInvoker =
            _handlerInvokerCache.GetOrAdd(
                 cacheKey,
                 key => CreateHandlerInvoker(requestType, responseType)
            );
        // Terminal delegate that invokes the handler and returns boxed result
        RequestHandlerDelegate<TResponse> terminal = async () =>
        {
            object boxed = await handlerInvoker(_provider, request, cancellationToken).ConfigureAwait(false);
            return (TResponse)boxed!;
        };
        // Resolve behaviors IEnumerable<IPipelineBehavior<requestType, responseType>>
        Type behaviorGenericType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);
        Type enumerableBehaviorType = typeof(IEnumerable<>).MakeGenericType(behaviorGenericType);
        object? behaviorsObj = _provider.GetService(enumerableBehaviorType);
        RequestHandlerDelegate<TResponse> pipeline = terminal;
        if (behaviorsObj is System.Collections.IEnumerable behaviorsEnumerable)
        {
            // Build pipeline by wrapping `pipeline` with each behavior in reverse order
            var behaviorList = behaviorsEnumerable.Cast<object>().Reverse().ToList();
            foreach (object? behaviorInstance in behaviorList)
            {
                Type behaviorType = behaviorInstance.GetType();
                MethodInfo? methodInfo = _behaviorHandleMethodInfoCache.GetOrAdd(behaviorType,
                    t => t.GetMethod("Handle", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ) ?? throw new InvalidOperationException($"Behavior type {behaviorType.FullName} does not have a suitable Handle method.");

                RequestHandlerDelegate<TResponse> prev = pipeline;
                object behavior = behaviorInstance; // capture
                pipeline = () =>
                {
                    // Call methodInfo.Invoke(behavior, new object[] { request, cancellationToken, prev })
                    // It's okay to use MethodInfo.Invoke here because we've cached MethodInfo; the heavier cost (creating MethodInfo) is avoided.
                    var resultTask = (Task<TResponse>)methodInfo.Invoke(behavior, new object[] { request, cancellationToken, prev })!;
                    return resultTask;
                };
            }
        }
        // Execute pipeline and return typed response
        return await pipeline().ConfigureAwait(false);
    }
    /// <summary>
    /// Creates a compiled delegate that resolves the typed IRequestHandler<TRequest, TResponse>
    /// from IServiceProvider and invokes its Handle(TRequest, CancellationToken) method.
    /// The delegate has the signature:
    ///     Func<IServiceProvider, object(request), CancellationToken, Task<object(response)>>
    /// It boxes/unboxes for generality and is cached for performance.
    /// </summary>
    private static Func<IServiceProvider, object, CancellationToken, Task<object>> CreateHandlerInvoker(Type requestType, Type responseType)
    {
        // handlerType = IRequestHandler<requestType, responseType>
        Type handlerInterfaceType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
        // Build expressions:
        // (IServiceProvider provider, object requestObj, CancellationToken ct) => {
        //     var handler = provider.GetService(handlerInterfaceType);
        //     if (handler == null) throw ...
        //     return InvokeAndBoxAsync<TResponse>(((IRequestHandler<TReq,TResp>)handler).Handle((TReq)requestObj, ct));
        // }
        ParameterExpression providerParam = Expression.Parameter(typeof(IServiceProvider), "provider");
        ParameterExpression requestObjParam = Expression.Parameter(typeof(object), "requestObj");
        ParameterExpression ctParam = Expression.Parameter(typeof(CancellationToken), "ct");
        MethodInfo getServiceMethod = typeof(IServiceProvider).GetMethod("GetService", new[] { typeof(Type) })!;
        ParameterExpression handlerVar = Expression.Variable(typeof(object), "handlerObj");
        Expression assignHandler = Expression.Assign(handlerVar, Expression.Call(providerParam, getServiceMethod, Expression.Constant(handlerInterfaceType, typeof(Type))));
        ConstructorInfo invalidOpCtor = typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) })!;
        Expression throwIfNull = Expression.IfThen(
            Expression.Equal(handlerVar, Expression.Constant(null, typeof(object))),
            Expression.Throw(Expression.New(invalidOpCtor, Expression.Constant($"No handler registered for {requestType.FullName}")))
        );
        Expression handlerCast = Expression.Convert(handlerVar, handlerInterfaceType);
        Expression requestCast = Expression.Convert(requestObjParam, requestType);
        MethodInfo handleMethod = handlerInterfaceType.GetMethod("Handle")!; // Handle(TRequest, CancellationToken)
        Expression handleCall = Expression.Call(handlerCast, handleMethod, requestCast, ctParam); // returns Task<TResponse>
        MethodInfo helperMethod = typeof(SimpleMediator).GetMethod(nameof(InvokeAndBoxAsync), BindingFlags.NonPublic | BindingFlags.Static)!.MakeGenericMethod(responseType);
        Expression boxedTaskCall = Expression.Call(helperMethod, handleCall);
        Expression body = Expression.Block(
            new[] { handlerVar },
            assignHandler,
            throwIfNull,
            boxedTaskCall
        );
        var lambda =
            Expression.Lambda<Func<IServiceProvider, object, CancellationToken, Task<object>>>(
                body,
                providerParam, requestObjParam, ctParam);
        Func<IServiceProvider, object, CancellationToken, Task<object>> compiled = lambda.Compile();
        return compiled;
    }
    // Helper used by expression to await Task<T> and box result to object
    private static async Task<object> InvokeAndBoxAsync<TResponse>(Task<TResponse> task)
    {
        TResponse result = await task.ConfigureAwait(false);
        return (object)result!;
    }
}
