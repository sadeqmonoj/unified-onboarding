using System.Reflection;
using Platform.BuildingBlocks.CustomMediator;
using Platform.BuildingBlocks.Results;

namespace Platform.Infrastructure.CustomMediator.Behaviors;

public sealed class ErrorHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            // Only apply Result<T> mapping to Result<_> responses
            if (typeof(TResponse).IsGenericType &&
                typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                Type innerType = typeof(TResponse).GetGenericArguments()[0];

                // Create Error
                var error = Error.Server(ex.Message);

                // Call: Result<Inner>.Fail(error)
                MethodInfo? failMethod = typeof(Result<>)
                    .MakeGenericType(innerType)
                    .GetMethod("Fail", BindingFlags.Public | BindingFlags.Static);

                object resultObj = failMethod!.Invoke(null, new object[] { error })!;

                return (TResponse)resultObj;
            }

            throw;
        }
    }
}
