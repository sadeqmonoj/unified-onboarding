using FluentValidation;
using FluentValidation.Results;
using Platform.BuildingBlocks.CustomMediator;

namespace Platform.Infrastructure.CustomMediator.Behaviors;

internal class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
   where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            ValidationResult[] results = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = results.SelectMany(r => r.Errors).Where(f => f != null).ToList();
            if (failures.Any())
            {
                throw new ValidationException(failures);
            }
        }
        return await next();
    }
}
