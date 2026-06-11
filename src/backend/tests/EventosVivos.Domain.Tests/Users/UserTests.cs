using EventosVivos.Domain.Users;

namespace EventosVivos.Domain.Tests.Users;

public class UserTests
{
    [Fact]
    public void Create_normalizes_the_email_and_sets_the_fields()
    {
        var user = User.Create("  Admin@Example.COM ", "hashed", "Administrador", UserRole.Admin);

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("admin@example.com", user.Email);
        Assert.Equal("hashed", user.PasswordHash);
        Assert.Equal("Administrador", user.Name);
        Assert.Equal(UserRole.Admin, user.Role);
    }

    [Fact]
    public void Normalize_trims_and_lowercases()
    {
        Assert.Equal("user@example.com", User.Normalize("  USER@Example.com "));
    }
}
