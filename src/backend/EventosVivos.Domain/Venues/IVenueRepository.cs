namespace EventosVivos.Domain.Venues;

public interface IVenueRepository
{
    Task<Venue?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// RN02: marks the venue so that creating one of its events bumps the venue's optimistic
    /// concurrency token (xmin), serializing concurrent event creations for the same venue.
    /// </summary>
    void Touch(Venue venue);

    /// <summary>
    /// Optionally filtered search used to populate venue selectors. Returns a limited set of
    /// matches; no total count is computed, since selectors do not paginate.
    /// </summary>
    Task<IReadOnlyList<Venue>> SearchAsync(string? term, int limit, CancellationToken cancellationToken);
}
