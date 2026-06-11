namespace EventosVivos.Domain.Users;

public interface IUserRepository
{
    void Add(User user);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(string email, CancellationToken cancellationToken);
}
