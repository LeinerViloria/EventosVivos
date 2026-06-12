namespace EventosVivos.Application.Features.Reports.OccupancyReport;

/// <summary>Renders the occupancy report (RF-06) as a PDF document.</summary>
public interface IOccupancyReportPdfGenerator
{
    byte[] Generate(IReadOnlyList<OccupancyReportItem> items);
}
