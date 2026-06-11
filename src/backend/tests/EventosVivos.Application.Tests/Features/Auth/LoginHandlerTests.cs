using EventosVivos.Application.Abstractions;
using EventosVivos.Application.Features.Auth.Login;
using EventosVivos.Domain.Users;
using NSubstitute;

namespace EventosVivos.Application.Tests.Features.Auth;

public class LoginHandlerTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly ISessionStore _sessions = Substitute.For<ISessionStore>();
    private readonly IPermissionStore _permissions = Substitute.For<IPermissionStore>();
    private readonly ITokenService _tokens = Substitute.For<ITokenService>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        _clock.UtcNow.Returns(new DateTimeOffset(2026, 6, 15, 12, 0, 0, TimeSpan.Zero));
        _tokens.IdentityTokenLifetime.Returns(TimeSpan.FromMinutes(15));
        _handler = new LoginHandler(_users, _hasher, _sessions, _permissions, _tokens, _clock);
    }

    private static User AnAdmin() =>
        User.Create("admin@eventosvivos.dev", "stored-hash", "Administrador", UserRole.Admin);

    [Fact]
    public async Task Fails_when_the_user_does_not_exist()
    {
        _users.GetByEmailAsync("ghost@example.com", Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _handler.Handle(
            new LoginCommand("ghost@example.com", "secret"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("AUTH_INVALID_CREDENTIALS", result.Error.Code);
        await _sessions.DidNotReceive().CreateAsync(
            Arg.Any<Guid>(), Arg.Any<SessionData>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Fails_when_the_password_is_wrong()
    {
        var user = AnAdmin();
        _users.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        _hasher.Verify("wrong", user.PasswordHash).Returns(false);

        var result = await _handler.Handle(new LoginCommand(user.Email, "wrong"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("AUTH_INVALID_CREDENTIALS", result.Error.Code);
    }

    [Fact]
    public async Task Issues_tokens_and_creates_a_session_on_success()
    {
        var user = AnAdmin();
        _users.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        _hasher.Verify("right", user.PasswordHash).Returns(true);
        _permissions.GetPermissionsAsync(UserRole.Admin, Arg.Any<CancellationToken>())
            .Returns(new[] { "events.create" });
        _tokens.CreateIdentityToken(user.Id, UserRole.Admin, Arg.Any<Guid>()).Returns("identity-token");
        _tokens.CreatePermissionsToken(UserRole.Admin, user.Name, user.Email, Arg.Any<IReadOnlyList<string>>())
            .Returns("permissions-token");

        var result = await _handler.Handle(new LoginCommand(user.Email, "right"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("identity-token", result.Value.IdentityToken);
        Assert.Equal("permissions-token", result.Value.PermissionsToken);
        await _sessions.Received(1).CreateAsync(
            Arg.Any<Guid>(),
            Arg.Is<SessionData>(session => session.UserId == user.Id && session.Role == UserRole.Admin),
            TimeSpan.FromMinutes(15),
            Arg.Any<CancellationToken>());
    }
}
