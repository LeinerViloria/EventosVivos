using EventosVivos.Application.Features.Reports.OccupancyReport;
using EventosVivos.Application.Security;
using Mediator;

namespace EventosVivos.Api.Endpoints.Reports;

public sealed class OccupancyReportEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/reports/occupancy", async (
            ISender sender,
            CancellationToken cancellationToken,
            int? page,
            int? pageSize) =>
        {
            var query = new OccupancyReportQuery(page ?? 1, pageSize ?? 10);
            var result = await sender.Send(query, cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Reports")
        .RequireAuthorization(Permissions.ReportsRead);
    }
}
