using EventosVivos.Application.Features.Auth.Login;
using EventosVivos.Domain.Common;
using Mediator;

namespace EventosVivos.Application.Features.Auth.Register;

/// <summary>Public self-registration. New accounts are always created with the regular user role.</summary>
public sealed record RegisterCommand(string Name, string Email, string Password)
    : IRequest<Result<LoginResponse>>;
