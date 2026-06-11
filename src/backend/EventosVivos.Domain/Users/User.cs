namespace EventosVivos.Domain.Users;

/// <summary>
/// An application user who can sign in. The password is stored only as a hash; the domain never
/// holds the plain text. Email is normalized to lowercase so lookups are case-insensitive.
/// </summary>
public sealed class User
{
    private User()
    {
        // Required by EF Core.
    }

    private User(Guid id, string email, string passwordHash, string name, UserRole role)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        Name = name;
        Role = role;
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; } = null!;

    public string PasswordHash { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public UserRole Role { get; private set; }

    public static User Create(string email, string passwordHash, string name, UserRole role) =>
        new(Guid.CreateVersion7(), Normalize(email), passwordHash, name, role);

    public static string Normalize(string email) => email.Trim().ToLowerInvariant();
}
