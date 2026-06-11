using EventosVivos.Application.Abstractions;
using EventosVivos.Application.Features.Auth.Login;
using EventosVivos.Domain.Common;
using EventosVivos.Domain.Users;
using Mediator;

namespace EventosVivos.Application.Features.Auth.Register;

public sealed class RegisterHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    ISessionStore sessions,
    IPermissionStore permissions,
    ITokenService tokens,
    IClock clock) : IRequestHandler<RegisterCommand, Result<LoginResponse>>
{
    public async ValueTask<Result<LoginResponse>> Handle(
        RegisterCommand command,
        CancellationToken cancellationToken)
    {
        if (await users.ExistsAsync(command.Email, cancellationToken))
        {
            return Result.Failure<LoginResponse>(AuthErrors.EmailAlreadyRegistered);
        }

        var user = User.Create(
            command.Email, passwordHasher.Hash(command.Password), command.Name.Trim(), UserRole.User);
        users.Add(user);

        var response = await AuthTokenIssuer.IssueAsync(
            user, sessions, permissions, tokens, clock, cancellationToken);

        return Result.Success(response);
    }
}
