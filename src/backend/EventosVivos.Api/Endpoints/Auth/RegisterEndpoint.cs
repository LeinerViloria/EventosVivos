using EventosVivos.Api.Errors;
using EventosVivos.Application.Features.Auth.Register;
using Mediator;

namespace EventosVivos.Api.Endpoints.Auth;

public sealed class RegisterEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/register", async (
            RegisterRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new RegisterCommand(request.Name, request.Email, request.Password);
            var result = await sender.Send(command, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.Error.ToProblemResult();
        })
        .WithTags("Auth")
        .AllowAnonymous();
    }
}

public sealed record RegisterRequest(string Name, string Email, string Password);
