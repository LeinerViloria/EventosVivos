using EventosVivos.Application.Abstractions;
using EventosVivos.Application.Security;
using EventosVivos.Domain.Users;
using Microsoft.Extensions.DependencyInjection;

namespace EventosVivos.Infrastructure;

/// <summary>
/// Seeds the sign-in data the statement does not provide: a development admin and a regular user
/// (idempotent), plus the role-to-permissions catalog in Redis. Without it there would be no way
/// to log in or exercise the app end to end.
/// </summary>
public static class AuthDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var provider = scope.ServiceProvider;

        var users = provider.GetRequiredService<IUserRepository>();
        var hasher = provider.GetRequiredService<IPasswordHasher>();
        var unitOfWork = provider.GetRequiredService<IUnitOfWork>();

        await SeedUserAsync(
            users, hasher, "admin@eventosvivos.dev", "Admin123*", "Administrador", UserRole.Admin, cancellationToken);
        await SeedUserAsync(
            users, hasher, "usuario@eventosvivos.dev", "Usuario123*", "Usuario", UserRole.User, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var permissions = provider.GetRequiredService<IPermissionStore>();
        await permissions.SeedCatalogAsync(RolePermissions.Catalog, cancellationToken);
    }

    private static async Task SeedUserAsync(
        IUserRepository users,
        IPasswordHasher hasher,
        string email,
        string password,
        string name,
        UserRole role,
        CancellationToken cancellationToken)
    {
        if (!await users.ExistsAsync(email, cancellationToken))
        {
            users.Add(User.Create(email, hasher.Hash(password), name, role));
        }
    }
}
