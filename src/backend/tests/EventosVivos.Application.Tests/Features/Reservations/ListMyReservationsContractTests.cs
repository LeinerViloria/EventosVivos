using EventosVivos.Application.Features.Reservations.ListMyReservations;

namespace EventosVivos.Application.Tests.Features.Reservations;

public class ListMyReservationsContractTests
{
    [Fact]
    public void Query_defaults_to_the_first_page_with_no_status_filter()
    {
        var userId = Guid.CreateVersion7();
        var query = new ListMyReservationsQuery(userId);

        Assert.Equal(userId, query.UserId);
        Assert.Null(query.Status);
        Assert.Equal(1, query.Page);
        Assert.Equal(10, query.PageSize);
    }

    [Fact]
    public void Query_carries_all_provided_parameters()
    {
        var userId = Guid.CreateVersion7();
        var query = new ListMyReservationsQuery(userId, Domain.Reservations.ReservationStatus.Confirmed, Page: 3, PageSize: 25);

        Assert.Equal(userId, query.UserId);
        Assert.Equal(Domain.Reservations.ReservationStatus.Confirmed, query.Status);
        Assert.Equal(3, query.Page);
        Assert.Equal(25, query.PageSize);
    }
}
