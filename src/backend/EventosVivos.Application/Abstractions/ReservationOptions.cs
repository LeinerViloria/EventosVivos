namespace EventosVivos.Application.Abstractions;

/// <summary>How long a pending-payment reservation holds its tickets before it expires.</summary>
public sealed class ReservationOptions
{
    public int TtlMinutes { get; init; } = 15;
}
