using EventosVivos.Application.Abstractions;

namespace EventosVivos.Infrastructure.Time;

internal sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
