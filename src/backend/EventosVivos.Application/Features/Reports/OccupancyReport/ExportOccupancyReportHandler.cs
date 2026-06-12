using Mediator;

namespace EventosVivos.Application.Features.Reports.OccupancyReport;

public sealed class ExportOccupancyReportHandler(
    IOccupancyReportReader reader,
    IOccupancyReportPdfGenerator pdfGenerator)
    : IRequestHandler<ExportOccupancyReportQuery, byte[]>
{
    public async ValueTask<byte[]> Handle(
        ExportOccupancyReportQuery query,
        CancellationToken cancellationToken)
    {
        var items = await reader.GetAllAsync(cancellationToken);
        return pdfGenerator.Generate(items);
    }
}
