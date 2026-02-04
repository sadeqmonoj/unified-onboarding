
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

        return Results.Json(
            new
            {
                error = result.Error!.Code,
                message = result.Error!.Message
            },
            statusCode: result.Error.HttpStatusCode
        );
    }
}
