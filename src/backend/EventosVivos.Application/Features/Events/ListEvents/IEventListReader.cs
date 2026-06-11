using EventosVivos.Application.Abstractions;

namespace EventosVivos.Application.Features.Events.ListEvents;

/// <summary>
/// Read model for the events listing. Implemented by the infrastructure with EF Core, so the
/// database resolves the filters, ordering, count and pagination.
/// </summary>
public interface IEventListReader
{
    Task<PagedResult<EventListItem>> ListAsync(ListEventsQuery query, CancellationToken cancellationToken);
}
