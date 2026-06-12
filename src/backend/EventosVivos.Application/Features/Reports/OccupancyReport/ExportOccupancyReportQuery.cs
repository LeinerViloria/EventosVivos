using Mediator;

namespace EventosVivos.Application.Features.Reports.OccupancyReport;

/// <summary>Builds the occupancy report (RF-06) as a PDF (admin). Returns the document bytes.</summary>
public sealed record ExportOccupancyReportQuery : IRequest<byte[]>;
