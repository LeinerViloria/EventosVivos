using EventosVivos.Domain.Common;

namespace EventosVivos.Domain.Events;

public static class EventErrors
{
    public static Error CapacityExceedsVenue(int venueCapacity) =>
        new("EVENT_CAPACITY_EXCEEDS_VENUE",
            new Dictionary<string, object?> { ["venueCapacity"] = venueCapacity });

    public static readonly Error EndNotAfterStart = new("EVENT_END_NOT_AFTER_START");

    public static readonly Error WeekendNightStart = new("EVENT_WEEKEND_NIGHT_START");
}
