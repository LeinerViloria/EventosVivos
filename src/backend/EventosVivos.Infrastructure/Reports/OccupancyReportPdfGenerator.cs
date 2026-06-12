using System.Globalization;
using EventosVivos.Application.Features.Reports.OccupancyReport;
using EventosVivos.Domain.Events;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EventosVivos.Infrastructure.Reports;

/// <summary>
/// Renders the occupancy report as a PDF with QuestPDF. The document is read by people, so its
/// labels are in Spanish (es-CO), unlike the API responses, which only carry codes.
/// </summary>
internal sealed class OccupancyReportPdfGenerator : IOccupancyReportPdfGenerator
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    static OccupancyReportPdfGenerator() => QuestPDF.Settings.License = LicenseType.Community;

    public byte[] Generate(IReadOnlyList<OccupancyReportItem> items) =>
        Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Margin(36);
                page.Size(PageSizes.A4.Landscape());
                page.DefaultTextStyle(text => text.FontSize(9));

                page.Header().Column(column =>
                {
                    column.Item().Text("Reporte de ocupación").FontSize(16).Bold();
                    column.Item().Text("EventosVivos").FontColor(Colors.Grey.Medium);
                });

                page.Content().PaddingVertical(12).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(1.5f);
                    });

                    table.Header(header =>
                    {
                        HeaderCell(header, "Evento");
                        HeaderCell(header, "Capacidad");
                        HeaderCell(header, "Vendidas");
                        HeaderCell(header, "Disponibles");
                        HeaderCell(header, "Ocupación");
                        HeaderCell(header, "Ingresos");
                        HeaderCell(header, "Estado");
                    });

                    foreach (var item in items)
                    {
                        table.Cell().Text(item.EventTitle);
                        Number(table, item.Capacity);
                        Number(table, item.SoldTickets);
                        Number(table, item.AvailableTickets);
                        table.Cell().AlignRight()
                            .Text(item.OccupancyPercent.ToString("0.0", Culture) + " %");
                        table.Cell().AlignRight()
                            .Text("COP $" + item.Revenue.ToString("#,##0", Culture));
                        table.Cell().Text(StatusLabel(item.Status));
                    }
                });

                page.Footer().AlignRight().Text(text =>
                {
                    text.Span("Página ");
                    text.CurrentPageNumber();
                    text.Span(" de ");
                    text.TotalPages();
                });
            });
        }).GeneratePdf();

    private static void HeaderCell(TableCellDescriptor header, string label) =>
        header.Cell().BorderBottom(1).PaddingBottom(4).Text(label).Bold();

    private static void Number(TableDescriptor table, int value) =>
        table.Cell().AlignRight().Text(value.ToString(Culture));

    private static string StatusLabel(EventStatus status) => status switch
    {
        EventStatus.Active => "Activo",
        EventStatus.Cancelled => "Cancelado",
        EventStatus.Completed => "Completado",
        _ => status.ToString(),
    };
}
