using EventosVivos.Application.Abstractions;
using EventosVivos.Domain.Common;
using EventosVivos.Domain.Users;
using Mediator;

namespace EventosVivos.Application.Features.Auth.Login;

public sealed class LoginHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    ISessionStore sessions,
    IPermissionStore permissions,
    ITokenService tokens,
    IClock clock) : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async ValueTask<Result<LoginResponse>> Handle(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        var user = await users.GetByEmailAsync(command.Email, cancellationToken);
        if (user is null || !passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            return Result.Failure<LoginResponse>(AuthErrors.InvalidCredentials);
        }

        var sessionId = Guid.CreateVersion7();
        await sessions.CreateAsync(
            sessionId,
            new SessionData(user.Id, user.Role, clock.UtcNow),
            tokens.IdentityTokenLifetime,
            cancellationToken);

        var rolePermissions = await permissions.GetPermissionsAsync(user.Role, cancellationToken);
        var identityToken = tokens.CreateIdentityToken(user.Id, user.Role, sessionId);
        var permissionsToken = tokens.CreatePermissionsToken(user.Role, user.Name, rolePermissions);

        return Result.Success(new LoginResponse(identityToken, permissionsToken));
    }
}
