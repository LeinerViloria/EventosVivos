using System.Text.Json;
using EventosVivos.Application.Abstractions;
using EventosVivos.Domain.Users;
using StackExchange.Redis;

namespace EventosVivos.Infrastructure.Security;

internal sealed class RedisPermissionStore(IConnectionMultiplexer redis) : IPermissionStore
{
    private static string Key(UserRole role) => $"permissions:{(byte)role}";

    public async Task SeedCatalogAsync(
        IReadOnlyDictionary<UserRole, IReadOnlyList<string>> catalog,
        CancellationToken cancellationToken)
    {
        var database = redis.GetDatabase();
        foreach (var (role, permissions) in catalog)
        {
            await database.StringSetAsync(Key(role), JsonSerializer.Serialize(permissions));
        }
    }

    public async Task<IReadOnlyList<string>> GetPermissionsAsync(
        UserRole role,
        CancellationToken cancellationToken)
    {
        var value = await redis.GetDatabase().StringGetAsync(Key(role));
        return value.IsNullOrEmpty
            ? []
            : JsonSerializer.Deserialize<IReadOnlyList<string>>(value.ToString()) ?? [];
    }
}
