using EventosVivos.Application.Abstractions;
using Mediator;

namespace EventosVivos.Application.Features.Events.ListEvents;

public sealed class ListEventsHandler(IEventListReader reader)
    : IRequestHandler<ListEventsQuery, PagedResult<EventListItem>>
{
    public ValueTask<PagedResult<EventListItem>> Handle(
        ListEventsQuery query,
        CancellationToken cancellationToken) =>
        new(reader.ListAsync(query, cancellationToken));
}
