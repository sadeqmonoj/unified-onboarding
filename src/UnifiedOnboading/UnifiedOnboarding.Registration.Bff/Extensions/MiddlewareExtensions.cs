using Platform.Infrastructure.Extensions;

namespace UnifiedOnboarding.Registration.Bff.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseBffMiddlewares(this IApplicationBuilder app)
    {
        app.UseCorrelationId();
        app.UseExceptionHandlerMiddleware();
        

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseSwagger();                        // Generates Swagger JSON at /swagger/v1/swagger.json
        app.UseSwaggerUI(c =>                    // Enables Swagger UI
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Registration API V1");
            c.RoutePrefix = string.Empty;        // Serve Swagger UI at root: http://localhost:5000/
        });

        return app;
    }
}
