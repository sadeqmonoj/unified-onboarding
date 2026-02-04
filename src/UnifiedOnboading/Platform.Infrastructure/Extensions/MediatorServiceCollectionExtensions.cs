using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Platform.BuildingBlocks.CustomMediator;
using Platform.Infrastructure.CustomMediator;
using Platform.Infrastructure.CustomMediator.Behaviors;

namespace Platform.Infrastructure.Extensions;

public static class MediatorServiceCollectionExtensions
{
    public static IServiceCollection AddSimpleMediator(this IServiceCollection services, Assembly[] scanAssemblies)
    {
        services.AddMemoryCache(); // <-- ADD THIS
        // Mediator itself
        services.AddScoped<IMediator, SimpleMediator>();

        // Register handlers: IRequestHandler<,>
        foreach (Assembly asm in scanAssemblies)
        {
            var handlerTypes = asm.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .SelectMany(t => t.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                    .Select(i => new { Impl = t, Service = i }));

            foreach (var pair in handlerTypes)
            {
                services.AddTransient(pair.Service, pair.Impl);
            }
        }

        // Register pipeline behaviors (open generic)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CacheBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ErrorHandlingBehavior<,>));


        // add more behaviors here

        // Register validators (FluentValidation scans assemblies)
        services.AddValidatorsFromAssemblies(scanAssemblies);

        return services;
    }
}
