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

        var response = await AuthTokenIssuer.IssueAsync(
            user, sessions, permissions, tokens, clock, cancellationToken);

        return Result.Success(response);
    }
}
