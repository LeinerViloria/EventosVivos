using EventosVivos.Domain.Common;
using Mediator;

namespace EventosVivos.Application.Features.Auth.Logout;

public sealed record LogoutCommand(Guid SessionId) : IRequest<Result>;
