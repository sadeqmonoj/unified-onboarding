using System.Diagnostics;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;


namespace Platform.Infrastructure.Middlewares;

public class ExceptionMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException vex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/problem+json";

            var errors = vex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            var problemDetails = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred."
            };

            await context.Response.WriteAsJsonAsync(problemDetails);
            return;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unhandled exception");
            string traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
            var payload = new { message = "Internal error", traceId };
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(payload);
        }
    }
}
