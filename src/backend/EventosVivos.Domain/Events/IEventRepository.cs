namespace EventosVivos.Domain.Events;

public interface IEventRepository
{
    void Add(Event @event);

    /// <summary>Loads a tracked event so changes to its ticket counter are saved under xmin.</summary>
    Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// RN02: indicates whether the venue already has an active event whose schedule overlaps
    /// the given time range.
    /// </summary>
    Task<bool> HasOverlappingActiveEventAsync(
        Guid venueId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken);
}
