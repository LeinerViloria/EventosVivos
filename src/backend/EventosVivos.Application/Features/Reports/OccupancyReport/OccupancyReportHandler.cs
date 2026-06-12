using EventosVivos.Application.Abstractions;
using Mediator;

namespace EventosVivos.Application.Features.Reports.OccupancyReport;

public sealed class OccupancyReportHandler(IOccupancyReportReader reader)
    : IRequestHandler<OccupancyReportQuery, PagedResult<OccupancyReportItem>>
{
    public ValueTask<PagedResult<OccupancyReportItem>> Handle(
        OccupancyReportQuery query,
        CancellationToken cancellationToken) =>
        new(reader.GetAsync(query, cancellationToken));
}
