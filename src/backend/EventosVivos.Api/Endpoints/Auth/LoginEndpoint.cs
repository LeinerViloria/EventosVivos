using EventosVivos.Api.Errors;
using EventosVivos.Application.Features.Auth.Login;
using Mediator;

namespace EventosVivos.Api.Endpoints.Auth;

public sealed class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/login", async (
            LoginRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(new LoginCommand(request.Email, request.Password), cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.Error.ToProblemResult();
        })
        .WithTags("Auth")
        .AllowAnonymous();
    }
}

public sealed record LoginRequest(string Email, string Password);
