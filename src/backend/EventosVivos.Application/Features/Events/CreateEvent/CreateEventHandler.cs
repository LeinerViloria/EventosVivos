using EventosVivos.Domain.Common;
using EventosVivos.Domain.Events;
using EventosVivos.Domain.Venues;
using Mediator;

namespace EventosVivos.Application.Features.Events.CreateEvent;

public sealed class CreateEventHandler(IVenueRepository venues, IEventRepository events)
    : IRequestHandler<CreateEventCommand, Result<CreateEventResponse>>
{
    public async ValueTask<Result<CreateEventResponse>> Handle(
        CreateEventCommand command,
        CancellationToken cancellationToken)
    {
        var venue = await venues.GetByIdAsync(command.VenueId, cancellationToken);
        if (venue is null)
        {
            return Result.Failure<CreateEventResponse>(EventErrors.VenueNotFound);
        }

        // RN02: two active events cannot share a venue with overlapping schedules.
        var overlaps = await events.HasOverlappingActiveEventAsync(
            command.VenueId, command.StartsAt.UtcDateTime, command.EndsAt.UtcDateTime, cancellationToken);
        if (overlaps)
        {
            return Result.Failure<CreateEventResponse>(EventErrors.VenueScheduleOverlap);
        }

        var creation = Event.Create(
            command.Title,
            command.Description,
            venue,
            command.MaxCapacity,
            command.StartsAt,
            command.EndsAt,
            command.Price,
            command.Type);

        if (creation.IsFailure)
        {
            return Result.Failure<CreateEventResponse>(creation.Error);
        }

        events.Add(creation.Value);

        // Bump the venue's concurrency token so concurrent creations for the same venue are
        // serialized (RN02 under concurrency).
        venues.Touch(venue);

        return Result.Success(new CreateEventResponse(creation.Value.Id));
    }
}
