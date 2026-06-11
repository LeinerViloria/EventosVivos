using EventosVivos.Application.Abstractions;
using EventosVivos.Application.Features.Auth.Login;
using EventosVivos.Domain.Users;

namespace EventosVivos.Application.Features.Auth;

/// <summary>
/// Creates a live session in Redis and issues the identity and permissions tokens for a user.
/// Shared by sign-in and registration so both produce a signed-in session the same way.
/// </summary>
internal static class AuthTokenIssuer
{
    public static async Task<LoginResponse> IssueAsync(
        User user,
        ISessionStore sessions,
        IPermissionStore permissions,
        ITokenService tokens,
        IClock clock,
        CancellationToken cancellationToken)
    {
        var sessionId = Guid.CreateVersion7();
        await sessions.CreateAsync(
            sessionId,
            new SessionData(user.Id, user.Role, clock.UtcNow),
            tokens.IdentityTokenLifetime,
            cancellationToken);

        var rolePermissions = await permissions.GetPermissionsAsync(user.Role, cancellationToken);

        return new LoginResponse(
            tokens.CreateIdentityToken(user.Id, user.Role, sessionId),
            tokens.CreatePermissionsToken(user.Role, user.Name, user.Email, rolePermissions));
    }
}
