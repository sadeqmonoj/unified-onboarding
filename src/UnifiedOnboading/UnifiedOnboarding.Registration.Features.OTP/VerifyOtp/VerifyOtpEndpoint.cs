using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Platform.BuildingBlocks.CustomMediator;
using Platform.BuildingBlocks.Results;
using Platform.Infrastructure.Extensions;

namespace UnifiedOnboarding.Registration.Features.OTP.VerifyOtp;

public static class VerifyOtpEndpoint
{
    public static RouteGroupBuilder MapVerifyOtpEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("/VerifyOtp", async (IMediator mediator, VerifyOtpRequest request) =>
        {

            Result<VerifyOtpResponse> result = await mediator.Send(request);
            return result.ToApiResult();
        })
        .WithTags("Otp")
        .WithDefaultBffConfig()
        .RequireRateLimiting("OtpVerify")    
        .RequireRateLimiting("OtpPerPhone")  
        .Produces<VerifyOtpResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status429TooManyRequests)
        .WithName("VerifyOtp");

        return group;
    }
}
