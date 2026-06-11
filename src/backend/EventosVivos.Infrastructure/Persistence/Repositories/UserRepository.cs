using EventosVivos.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

internal sealed class UserRepository(EventosVivosDbContext context) : IUserRepository
{
    public void Add(User user) => context.Users.Add(user);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
        context.Users.FirstOrDefaultAsync(u => u.Email == User.Normalize(email), cancellationToken);

    public Task<bool> ExistsAsync(string email, CancellationToken cancellationToken) =>
        context.Users.AnyAsync(u => u.Email == User.Normalize(email), cancellationToken);
}
