using System.Text.Json;
using EventosVivos.Application.Abstractions;
using StackExchange.Redis;

namespace EventosVivos.Infrastructure.Security;

internal sealed class RedisSessionStore(IConnectionMultiplexer redis) : ISessionStore
{
    private static string Key(Guid sessionId) => $"session:{sessionId}";

    public Task CreateAsync(
        Guid sessionId,
        SessionData data,
        TimeSpan timeToLive,
        CancellationToken cancellationToken) =>
        redis.GetDatabase().StringSetAsync(Key(sessionId), JsonSerializer.Serialize(data), timeToLive);

    public async Task<SessionData?> GetAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var value = await redis.GetDatabase().StringGetAsync(Key(sessionId));
        return value.IsNullOrEmpty ? null : JsonSerializer.Deserialize<SessionData>(value.ToString());
    }

    public Task RemoveAsync(Guid sessionId, CancellationToken cancellationToken) =>
        redis.GetDatabase().KeyDeleteAsync(Key(sessionId));
}
