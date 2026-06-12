using EventosVivos.Application.Abstractions;

namespace EventosVivos.Infrastructure.Security;

internal sealed class ReservationCodeGenerator : IReservationCodeGenerator
{
    public string Generate() => $"EV-{Random.Shared.Next(0, 1_000_000):D6}";
}
