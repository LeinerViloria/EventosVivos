using EventosVivos.Domain.Users;

namespace EventosVivos.Application.Abstractions;

/// <summary>
/// The catalog that maps each role to its permissions, stored in Redis. Being unique for the
/// whole system, a change to a role's permissions takes effect immediately for every user.
/// </summary>
public interface IPermissionStore
{
    Task SeedCatalogAsync(
        IReadOnlyDictionary<UserRole, IReadOnlyList<string>> catalog,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> GetPermissionsAsync(UserRole role, CancellationToken cancellationToken);
}
