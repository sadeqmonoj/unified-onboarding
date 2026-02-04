using Microsoft.Extensions.Logging;
using Platform.BuildingBlocks.CustomMediator;

namespace Platform.Infrastructure.CustomMediator.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
       where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) => _logger = logger;

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        string requestName = typeof(TRequest).Name;
        _logger.LogInformation("Handling {RequestName} {@Request}", requestName, request);
        try
        {
            TResponse response = await next().ConfigureAwait(false);
            _logger.LogInformation("Handled {RequestName} {@Response}", requestName, response);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling {RequestName}", requestName);
            throw;
        }
    }
}
