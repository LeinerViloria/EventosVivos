namespace EventosVivos.Domain.Venues;

/// <summary>
/// A preexisting place where events are held. Reference data (seeded). Governs the
/// scheduling of its events (see RN02).
/// </summary>
public sealed class Venue
{
    private Venue()
    {
        // Required by EF Core.
    }

    public Venue(Guid id, string name, int capacity, string city)
    {
        Id = id;
        Name = name;
        Capacity = capacity;
        City = city;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public int Capacity { get; private set; }

    public string City { get; private set; } = null!;
}
