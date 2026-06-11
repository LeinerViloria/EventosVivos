using System.Security.Claims;
using EventosVivos.Api.Authentication;
using EventosVivos.Application.Features.Auth.Logout;
using Mediator;

namespace EventosVivos.Api.Endpoints.Auth;

public sealed class LogoutEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/logout", async (
            ClaimsPrincipal user,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var sessionId = user.GetSessionId();
            if (sessionId is null)
            {
                return Results.Unauthorized();
            }

            await sender.Send(new LogoutCommand(sessionId.Value), cancellationToken);
            return Results.NoContent();
        })
        .WithTags("Auth")
        .RequireAuthorization();
    }
}
