using EventosVivos.Application.Abstractions;
using Mediator;

namespace EventosVivos.Application.Features.Reports.OccupancyReport;

/// <summary>Occupancy report per event (RF-06, admin), aggregated and paginated on the server.</summary>
public sealed record OccupancyReportQuery(int Page = 1, int PageSize = 10)
    : IRequest<PagedResult<OccupancyReportItem>>;
