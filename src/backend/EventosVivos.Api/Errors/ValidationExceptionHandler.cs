using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EventosVivos.Api.Errors;

/// <summary>
/// Translates input validation failures into a 422 response with one entry per field,
/// each carrying its stable error code and parameters for the frontend to translate.
/// </summary>
internal sealed class ValidationExceptionHandler(IProblemDetailsService problemDetailsService)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
        {
            return false;
        }

        var errors = validationException.Errors
            .Select(failure => new
            {
                field = failure.PropertyName,
                errorCode = failure.ErrorCode,
                @params = failure.FormattedMessagePlaceholderValues,
            })
            .ToArray();

        httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = "Validation failed",
        };
        problemDetails.Extensions["errorKind"] = "validation";
        problemDetails.Extensions["errors"] = errors;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
        });
    }
}
