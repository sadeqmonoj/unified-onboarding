using System.Reflection;
using Microsoft.Extensions.Logging;
using Platform.BuildingBlocks.CustomMediator;
using Platform.BuildingBlocks.Results;

namespace Platform.Infrastructure.CustomMediator.Behaviors;

public sealed class ErrorHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ErrorHandlingBehavior<TRequest, TResponse>> _logger;

    public ErrorHandlingBehavior(ILogger<ErrorHandlingBehavior<TRequest, TResponse>> logger) =>
        _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        try
        {
            return await next();
        }
        catch (HttpRequestException ex)
        {
            // Downstream service unavailable
            _logger.LogError(ex, "Downstream service error for {Request}", typeof(TRequest).Name);
            return CreateFailedResult(Error.Server("SERVICE_UNAVAILABLE", "Downstream service unavailable"));
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            // Request timeout
            _logger.LogError(ex, "Request timeout for {Request}", typeof(TRequest).Name);
            return CreateFailedResult(Error.Server("TIMEOUT", "Request timeout"));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // User cancelled request
            _logger.LogInformation("Request cancelled by user for {Request}", typeof(TRequest).Name);
            throw; // Re-throw, let ASP.NET handle
        }
        catch (Exception ex)
        {
            // Unexpected error - this indicates a bug
            _logger.LogCritical(ex, "UNEXPECTED ERROR in {Request}", typeof(TRequest).Name);

            // In development, expose details
#if DEBUG
            return CreateFailedResult(Error.Server("INTERNAL_ERROR", ex.ToString()));
#else
            return CreateFailedResult(Error.Server("INTERNAL_ERROR", "An unexpected error occurred"));
#endif
        }
    }

    private TResponse CreateFailedResult(Error error)
    {
        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            Type innerType = typeof(TResponse).GetGenericArguments()[0];
            MethodInfo? failMethod = typeof(Result<>)
                .MakeGenericType(innerType)
                .GetMethod("Fail", BindingFlags.Public | BindingFlags.Static);
            return (TResponse)failMethod!.Invoke(null, new object[] { error })!;
        }

        throw new InvalidOperationException($"Cannot create failed result for type {typeof(TResponse).Name}");
    }
}
