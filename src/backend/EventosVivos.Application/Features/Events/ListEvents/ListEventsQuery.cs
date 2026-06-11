using EventosVivos.Application.Abstractions;
using EventosVivos.Domain.Events;
using Mediator;

namespace EventosVivos.Application.Features.Events.ListEvents;

/// <summary>
/// Lists events with optional filters (RF-02), paginated on the server. A null filter is ignored.
/// The date filter applies to the event's start; the title search is partial and case-insensitive.
/// </summary>
public sealed record ListEventsQuery(
    EventType? Type = null,
    EventStatus? Status = null,
    Guid? VenueId = null,
    DateTimeOffset? StartFrom = null,
    DateTimeOffset? StartTo = null,
    string? Title = null,
    int Page = 1,
    int PageSize = 10) : IRequest<PagedResult<EventListItem>>;
