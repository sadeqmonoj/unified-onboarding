using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Platform.BuildingBlocks.CustomMediator;
using Platform.BuildingBlocks.Results;
using UnifiedOnboarding.Registration.Features.OTP.SendOtp;
using UnifiedOnboarding.Registration.Features.OTP.VerifyOtp;

namespace UnifiedOnboarding.Registration.Features.OTP;

public static class DependencyInjection
{
    public static IServiceCollection AddOtpFeatures(this IServiceCollection services)
    {

        services.AddTransient<IRequestHandler<SendOtpRequest, Result<SendOtpResponse>>, SendOtpHandler>();
        services.AddTransient<IRequestHandler<VerifyOtpRequest, Result<VerifyOtpResponse>>, VerifyOtpHandler>();

        return services;
    }

    public static IEndpointRouteBuilder MapOtpFeature(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/Otp").WithTags("Otp");
        group.MapSendOtpEndpoint()
            .MapVerifyOtpEndpoint(); // Map the endpoint defined earlier

        return app;
    }
}
