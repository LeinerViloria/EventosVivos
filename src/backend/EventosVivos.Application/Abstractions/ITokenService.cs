using EventosVivos.Domain.Users;

namespace EventosVivos.Application.Abstractions;

/// <summary>
/// Issues the two JWTs. The identity token travels in every request to identify the user and
/// its session; the permissions token lives in the frontend only to show or hide UI.
/// </summary>
public interface ITokenService
{
    /// <summary>Lifetime of the identity token; the Redis session uses the same window.</summary>
    TimeSpan IdentityTokenLifetime { get; }

    string CreateIdentityToken(Guid userId, UserRole role, Guid sessionId);

    string CreatePermissionsToken(UserRole role, string name, IReadOnlyList<string> permissions);
}
