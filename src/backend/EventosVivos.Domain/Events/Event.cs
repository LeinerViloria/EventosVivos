using EventosVivos.Domain.Common;
using EventosVivos.Domain.Venues;

namespace EventosVivos.Domain.Events;

/// <summary>
/// An event held at a venue. Aggregate root that owns its ticket counter. Times are stored
/// in UTC; the local time used to evaluate the weekend-night rule (RN03) comes from the
/// client's time zone, carried by the <see cref="DateTimeOffset"/> inputs of <see cref="Create"/>.
/// </summary>
public sealed class Event
{
    private static readonly TimeSpan NightThreshold = new(22, 0, 0);

    private Event()
    {
        // Required by EF Core.
    }

    private Event(
        Guid id,
        string title,
        string description,
        Guid venueId,
        int maxCapacity,
        DateTime startUtc,
        DateTime endUtc,
        decimal price,
        EventType type)
    {
        Id = id;
        Title = title;
        Description = description;
        VenueId = venueId;
        MaxCapacity = maxCapacity;
        StartUtc = startUtc;
        EndUtc = endUtc;
        Price = price;
        Type = type;
        Status = EventStatus.Active;
        ReservedTickets = 0;
    }

    public Guid Id { get; private set; }

    public string Title { get; private set; } = null!;

    public string Description { get; private set; } = null!;

    public Guid VenueId { get; private set; }

    public int MaxCapacity { get; private set; }

    public DateTime StartUtc { get; private set; }

    public DateTime EndUtc { get; private set; }

    public decimal Price { get; private set; }

    public EventType Type { get; private set; }

    public EventStatus Status { get; private set; }

    public int ReservedTickets { get; private set; }

    public static Result<Event> Create(
        string title,
        string description,
        Venue venue,
        int maxCapacity,
        DateTimeOffset startsAt,
        DateTimeOffset endsAt,
        decimal price,
        EventType type)
    {
        // RN01: an event cannot exceed the capacity of its venue.
        if (maxCapacity > venue.Capacity)
        {
            return Result.Failure<Event>(EventErrors.CapacityExceedsVenue(venue.Capacity));
        }

        if (endsAt <= startsAt)
        {
            return Result.Failure<Event>(EventErrors.EndNotAfterStart);
        }

        // RN03: weekend events cannot start after 22:00 (evaluated in the client's local time).
        if (IsWeekendNightStart(startsAt))
        {
            return Result.Failure<Event>(EventErrors.WeekendNightStart);
        }

        var @event = new Event(
            Guid.CreateVersion7(),
            title,
            description,
            venue.Id,
            maxCapacity,
            startsAt.UtcDateTime,
            endsAt.UtcDateTime,
            price,
            type);

        return Result.Success(@event);
    }

    private static bool IsWeekendNightStart(DateTimeOffset startsAt)
    {
        var isWeekend = startsAt.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
        return isWeekend && startsAt.TimeOfDay > NightThreshold;
    }
}
