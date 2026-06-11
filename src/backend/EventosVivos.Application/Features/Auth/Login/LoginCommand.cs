using EventosVivos.Domain.Common;
using Mediator;

namespace EventosVivos.Application.Features.Auth.Login;

public sealed record LoginCommand(string Email, string Password) : IRequest<Result<LoginResponse>>;
