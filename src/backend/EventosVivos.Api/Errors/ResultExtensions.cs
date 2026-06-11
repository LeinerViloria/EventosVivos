using EventosVivos.Domain.Common;

namespace EventosVivos.Api.Errors;

/// <summary>
/// Translates a domain <see cref="Error"/> into an RFC 7807 ProblemDetails response. The body
/// carries the stable error code, its kind and its parameters; the frontend renders the text.
/// </summary>
public static class ResultExtensions
{
    public static IResult ToProblemResult(this Error error)
    {
        var statusCode = error.Code switch
        {
            "VENUE_NOT_FOUND" or "EVENT_NOT_FOUND" => StatusCodes.Status404NotFound,
            "AUTH_INVALID_CREDENTIALS" => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status409Conflict,
        };

        return Results.Problem(
            statusCode: statusCode,
            extensions: new Dictionary<string, object?>
            {
                ["errorCode"] = error.Code,
                ["errorKind"] = "business",
                ["params"] = error.Parameters,
            });
    }
}
