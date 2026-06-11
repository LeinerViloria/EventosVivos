using EventosVivos.Domain.Events;

namespace EventosVivos.Application.Features.Events.ListEvents;

/// <summary>A row of the events listing. Times are in UTC; the frontend localizes them.</summary>
public sealed record EventListItem(
    Guid Id,
    string Title,
    Guid VenueId,
    string VenueName,
    DateTime StartUtc,
    DateTime EndUtc,
    int MaxCapacity,
    int ReservedTickets,
    decimal Price,
    EventType Type,
    EventStatus Status);
