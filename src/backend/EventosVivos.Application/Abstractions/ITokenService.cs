using EventosVivos.Domain.Users;

namespace EventosVivos.Application.Abstractions;

/// <summary>
/// Issues the two JWTs. The identity token travels in every request to identify the user and
/// its session; the permissions token lives in the frontend only to show or hide UI.
/// </summary>
public interface ITokenService
{
    string CreateIdentityToken(Guid userId, UserRole role, Guid sessionId);

    string CreatePermissionsToken(UserRole role, IReadOnlyList<string> permissions);
}
