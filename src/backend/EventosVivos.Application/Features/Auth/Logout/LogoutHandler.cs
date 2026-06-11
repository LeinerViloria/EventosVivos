using EventosVivos.Application.Abstractions;
using EventosVivos.Domain.Common;
using Mediator;

namespace EventosVivos.Application.Features.Auth.Logout;

public sealed class LogoutHandler(ISessionStore sessions) : IRequestHandler<LogoutCommand, Result>
{
    public async ValueTask<Result> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        await sessions.RemoveAsync(command.SessionId, cancellationToken);
        return Result.Success();
    }
}
