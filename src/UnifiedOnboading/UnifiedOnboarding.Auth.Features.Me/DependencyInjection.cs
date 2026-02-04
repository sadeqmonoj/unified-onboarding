using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Platform.BuildingBlocks.CustomMediator;
using Platform.BuildingBlocks.Results;
using UnifiedOnboarding.Auth.Features.Me.Me;

namespace UnifiedOnboarding.Auth.Features.Me;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthFeature(this IServiceCollection services)
    {

        services.AddTransient<IRequestHandler<GetCurrentUserRequest, Result<GetCurrentUserResponse>>, GetCurrentUserHandler>();

        return services;
    }
    public static IEndpointRouteBuilder MapAuthFeature(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/auth").WithTags("Auth");
        group.MapGetCurrentUserEndpoint(); // Map the endpoint defined earlier
        return app;
    }
}
