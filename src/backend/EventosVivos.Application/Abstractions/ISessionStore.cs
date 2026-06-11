using EventosVivos.Domain.Users;

namespace EventosVivos.Application.Abstractions;

/// <summary>Data kept for a live session in Redis. Lets the backend revoke access immediately.</summary>
public sealed record SessionData(Guid UserId, UserRole Role, DateTimeOffset IssuedAt);

/// <summary>
/// Stores live sessions in Redis with a time to live. Removing a session revokes the user's
/// access at once, without waiting for the identity token to expire.
/// </summary>
public interface ISessionStore
{
    Task CreateAsync(Guid sessionId, SessionData data, TimeSpan timeToLive, CancellationToken cancellationToken);

    Task<SessionData?> GetAsync(Guid sessionId, CancellationToken cancellationToken);

    Task RemoveAsync(Guid sessionId, CancellationToken cancellationToken);
}
