using System.Reflection;
using FluentValidation;
using FluentValidation.Results;
using Platform.BuildingBlocks.CustomMediator;
using Platform.BuildingBlocks.Results;

namespace Platform.Infrastructure.CustomMediator.Behaviors;

internal class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
   where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        ValidationResult[] results = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken))
        );

        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
        {
            // Check if response type is Result<T>
            System.Type responseType = typeof(TResponse);

            if (responseType.IsGenericType &&
                responseType.GetGenericTypeDefinition() == typeof(Result<>))
            {
                // Build validation error
                var errors = failures
                    .GroupBy(f => f.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                var validationError = Error.Validation(
                    "VALIDATION_FAILED",
                    "One or more validation errors occurred",
                    errors
                );

                // Create Result<T>.Fail(validationError)
                System.Type innerType = responseType.GetGenericArguments()[0];
                MethodInfo? failMethod = typeof(Result<>)
                    .MakeGenericType(innerType)
                    .GetMethod("Fail", BindingFlags.Public | BindingFlags.Static);

                object? result = failMethod!.Invoke(null, new object[] { validationError });
                return (TResponse)result!;
            }

            // For non-Result types, throw as before
            throw new ValidationException(failures);
        }

        return await next();
    }
}

