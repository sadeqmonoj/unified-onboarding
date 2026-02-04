using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Platform.BuildingBlocks.CustomMediator;
using Platform.BuildingBlocks.Results;
using Platform.Infrastructure.Extensions;

namespace UnifiedOnboarding.Registration.Features.OTP.SendOtp;

public static class SendOtpEndpoint
{
    public static RouteGroupBuilder MapSendOtpEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("/SendOtp", async (IMediator mediator, SendOtpRequest request) =>
        {

            Result<SendOtpResponse> result = await mediator.Send(request);
            return result.ToApiResult();
        })
        .WithTags("Otp")
        .WithDefaultBffConfig()
        .Produces<SendOtpResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .WithName("SendOtp");

        return group;
    }
}
