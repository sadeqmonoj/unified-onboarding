using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Platform.BuildingBlocks.CustomMediator;
using Platform.BuildingBlocks.Results;
using Platform.Infrastructure.Extensions;

namespace UnifiedOnboarding.Auth.Features.Me.Me;

public static class GetCurrentUserEndpoint
{
    public static RouteGroupBuilder MapGetCurrentUserEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("/me", async (IMediator mediator) =>
        {
            Result<GetCurrentUserResponse> result = await mediator.Send(new GetCurrentUserRequest());
            return result.ToApiResult();
        })
        .WithTags("CurrentUser")
        .WithDefaultBffConfig()
        .RequireAuthorization("RequireUser")
        .Produces<GetCurrentUserResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .WithName("GetCurrentUser");

        return group;
    }
}
