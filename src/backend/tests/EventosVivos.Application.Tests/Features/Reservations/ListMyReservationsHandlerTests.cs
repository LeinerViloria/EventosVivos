using EventosVivos.Application.Abstractions;
using EventosVivos.Application.Features.Reservations.ListMyReservations;
using EventosVivos.Application.Features.Reservations.ListReservations;
using EventosVivos.Domain.Reservations;
using NSubstitute;

namespace EventosVivos.Application.Tests.Features.Reservations;

public class ListMyReservationsHandlerTests
{
    [Fact]
    public async Task Delegates_to_the_reader_and_returns_its_result()
    {
        var reader = Substitute.For<IMyReservationListReader>();
        var userId = Guid.CreateVersion7();
        var query = new ListMyReservationsQuery(userId, ReservationStatus.PendingPayment, Page: 2, PageSize: 5);
        var expected = new PagedResult<ReservationListItem>([], Total: 0, Page: 2, PageSize: 5);
        reader.ListAsync(query, Arg.Any<CancellationToken>()).Returns(expected);
        var handler = new ListMyReservationsHandler(reader);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Same(expected, result);
        await reader.Received(1).ListAsync(query, Arg.Any<CancellationToken>());
    }
}
