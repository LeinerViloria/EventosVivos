using EventosVivos.Application.Features.Reports.OccupancyReport;
using EventosVivos.Application.Security;
using Mediator;

namespace EventosVivos.Api.Endpoints.Reports;

public sealed class ExportOccupancyReportEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/reports/occupancy/pdf", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var pdf = await sender.Send(new ExportOccupancyReportQuery(), cancellationToken);
            return Results.File(pdf, "application/pdf", "occupancy-report.pdf");
        })
        .WithTags("Reports")
        .RequireAuthorization(Permissions.ReportsRead);
    }
}
