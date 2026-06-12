using EventosVivos.Application.Features.Reports.OccupancyReport;
using EventosVivos.Domain.Events;

namespace EventosVivos.Application.Tests.Features.Reports;

public class OccupancyReportContractTests
{
    [Fact]
    public void Query_defaults_to_the_first_page()
    {
        var query = new OccupancyReportQuery();

        Assert.Equal(1, query.Page);
        Assert.Equal(10, query.PageSize);
    }

    [Fact]
    public void Item_exposes_the_aggregated_occupancy_metrics()
    {
        var eventId = Guid.CreateVersion7();

        var item = new OccupancyReportItem(
            eventId, "Concierto", Capacity: 100, SoldTickets: 30,
            AvailableTickets: 60, OccupancyPercent: 40, Revenue: 1500m, EventStatus.Active);

        Assert.Equal(eventId, item.EventId);
        Assert.Equal("Concierto", item.EventTitle);
        Assert.Equal(100, item.Capacity);
        Assert.Equal(30, item.SoldTickets);
        Assert.Equal(60, item.AvailableTickets);
        Assert.Equal(40, item.OccupancyPercent);
        Assert.Equal(1500m, item.Revenue);
        Assert.Equal(EventStatus.Active, item.Status);
    }
}
