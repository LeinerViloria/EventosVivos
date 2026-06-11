using System.Security.Claims;
using EventosVivos.Application.Security;

namespace EventosVivos.Api.Authentication;

public static class ClaimsPrincipalExtensions
{
    /// <summary>The session id carried by the identity token, used to revoke it on logout.</summary>
    public static Guid? GetSessionId(this ClaimsPrincipal user) =>
        Guid.TryParse(user.FindFirst(AuthClaims.SessionId)?.Value, out var sessionId) ? sessionId : null;
}
