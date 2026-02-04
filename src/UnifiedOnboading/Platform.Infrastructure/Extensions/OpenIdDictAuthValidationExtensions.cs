using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Validation.AspNetCore;
using Platform.Contracts.Configurations;

namespace Platform.Infrastructure.Extensions;

public static class OpenIdDictAuthValidationExtensions
{
    public static IServiceCollection AddOpenIddictAuthValidation(this IServiceCollection services, IConfiguration configuration)
    {

        var authOptions = new AuthOptions();
        configuration.GetSection("Auth").Bind(authOptions);

        services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);

        // OpenIddict Validation (remote issuer)
        services.AddOpenIddict()
            .AddValidation(options =>
            {
                // URL of your SSO server
                options.SetIssuer(authOptions.Authority!);

                // Accept tokens intended for this API
                options.AddAudiences(authOptions.Audience!);

                // REQUIRED for remote discovery + JWKS + metadata
                options.UseSystemNetHttp();

                // REQUIRED to integrate with ASP.NET Core
                options.UseAspNetCore();
            });

        services
           .AddAuthorization(options =>
           {
               options.AddPolicy("RequireUser", policy =>
               {
                   policy.RequireAuthenticatedUser();
                   policy.RequireClaim("sub");
               });
               options.AddPolicy("RequireAdmin", policy =>
               {
                   policy.RequireAuthenticatedUser();
                   policy.RequireClaim("role", "admin");
               });
           });

        return services;
    }
}
