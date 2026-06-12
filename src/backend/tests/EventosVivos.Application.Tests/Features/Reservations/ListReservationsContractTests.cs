using EventosVivos.Application.Features.Reservations.ListReservations;
using EventosVivos.Domain.Reservations;

namespace EventosVivos.Application.Tests.Features.Reservations;

public class ListReservationsContractTests
{
    [Fact]
    public void Query_defaults_to_the_first_page_with_no_status_filter()
    {
        var query = new ListReservationsQuery();

        Assert.Null(query.Status);
        Assert.Equal(1, query.Page);
        Assert.Equal(10, query.PageSize);
    }

    [Fact]
    public void ListItem_exposes_the_projected_reservation_and_event_fields()
    {
        var id = Guid.CreateVersion7();
        var eventId = Guid.CreateVersion7();
        var created = new DateTime(2026, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        var item = new ReservationListItem(
            id, eventId, "Concierto", "Ana", "ana@example.com", 2,
            ReservationStatus.Confirmed, "EV-123456", created, created.AddMinutes(15));

        Assert.Equal(id, item.Id);
        Assert.Equal(eventId, item.EventId);
        Assert.Equal("Concierto", item.EventTitle);
        Assert.Equal("Ana", item.BuyerName);
        Assert.Equal("ana@example.com", item.BuyerEmail);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(ReservationStatus.Confirmed, item.Status);
        Assert.Equal("EV-123456", item.ConfirmationCode);
        Assert.Equal(created, item.CreatedAtUtc);
    }
}
