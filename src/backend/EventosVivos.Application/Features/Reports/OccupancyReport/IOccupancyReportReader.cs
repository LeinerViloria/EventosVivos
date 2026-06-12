using EventosVivos.Application.Abstractions;

namespace EventosVivos.Application.Features.Reports.OccupancyReport;

/// <summary>Read model for the occupancy report, implemented with EF Core aggregations.</summary>
public interface IOccupancyReportReader
{
    Task<PagedResult<OccupancyReportItem>> GetAsync(
        OccupancyReportQuery query,
        CancellationToken cancellationToken);
}
