using EventosVivos.Application.Abstractions;
using EventosVivos.Application.Features.Auth.Register;
using EventosVivos.Domain.Users;
using NSubstitute;

namespace EventosVivos.Application.Tests.Features.Auth;

public class RegisterHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly ISessionStore _sessions = Substitute.For<ISessionStore>();
    private readonly IPermissionStore _permissions = Substitute.For<IPermissionStore>();
    private readonly ITokenService _tokens = Substitute.For<ITokenService>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly RegisterHandler _handler;

    public RegisterHandlerTests()
    {
        _clock.UtcNow.Returns(new DateTimeOffset(2026, 6, 15, 12, 0, 0, TimeSpan.Zero));
        _tokens.IdentityTokenLifetime.Returns(TimeSpan.FromMinutes(15));
        _handler = new RegisterHandler(_users, _hasher, _sessions, _permissions, _tokens, _clock);
    }

    [Fact]
    public async Task Fails_when_the_email_is_already_registered()
    {
        _users.ExistsAsync("taken@example.com", Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.Handle(
            new RegisterCommand("Taken", "taken@example.com", "Password1"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("AUTH_EMAIL_ALREADY_REGISTERED", result.Error.Code);
        _users.DidNotReceive().Add(Arg.Any<User>());
    }

    [Fact]
    public async Task Creates_a_regular_user_and_issues_tokens()
    {
        _users.ExistsAsync("new@example.com", Arg.Any<CancellationToken>()).Returns(false);
        _hasher.Hash("Password1").Returns("hashed");
        _permissions.GetPermissionsAsync(UserRole.User, Arg.Any<CancellationToken>())
            .Returns(new[] { "events.read" });
        _tokens.CreateIdentityToken(Arg.Any<Guid>(), UserRole.User, Arg.Any<Guid>()).Returns("identity-token");
        _tokens.CreatePermissionsToken(UserRole.User, "Nueva Persona", Arg.Any<IReadOnlyList<string>>())
            .Returns("permissions-token");

        var result = await _handler.Handle(
            new RegisterCommand("Nueva Persona", "new@example.com", "Password1"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("identity-token", result.Value.IdentityToken);
        Assert.Equal("permissions-token", result.Value.PermissionsToken);
        _users.Received(1).Add(Arg.Is<User>(u => u.Role == UserRole.User && u.Email == "new@example.com"));
        await _sessions.Received(1).CreateAsync(
            Arg.Any<Guid>(), Arg.Any<SessionData>(), TimeSpan.FromMinutes(15), Arg.Any<CancellationToken>());
    }
}
