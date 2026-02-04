using Microsoft.AspNetCore.Http;
using Platform.BuildingBlocks.Results;

namespace Platform.Infrastructure.Extensions;

public static class ResultExtensions
{
    public static IResult ToApiResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        Error error = result.Error!;

        object payload;
        if (error.Type == ErrorType.Validation)
        {
            payload = new
            {
                error = error.Code,
                message = error.Message,
                errors = error.ValidationErrors
            };
        }
        else
        {
            payload = new
            {
                error = error.Code,
                message = error.Message
            };
        }

        return Results.Json(
            payload,
            statusCode: error.Type switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.RateLimit => StatusCodes.Status429TooManyRequests,
                ErrorType.Server => StatusCodes.Status500InternalServerError,
                _ => StatusCodes.Status500InternalServerError
            }
        );
    }
}
