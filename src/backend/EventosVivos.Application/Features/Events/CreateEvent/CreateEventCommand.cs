using EventosVivos.Domain.Common;
using EventosVivos.Domain.Events;
using Mediator;

namespace EventosVivos.Application.Features.Events.CreateEvent;

public sealed record CreateEventCommand(
    string Title,
    string Description,
    Guid VenueId,
    int MaxCapacity,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    decimal Price,
    EventType Type) : IRequest<Result<CreateEventResponse>>;
