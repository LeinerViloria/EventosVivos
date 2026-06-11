namespace EventosVivos.Application.Abstractions;

/// <summary>Provides the current time in UTC. Injectable so it can be controlled in tests.</summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
