using EventosVivos.Application.Abstractions;
using EventosVivos.Application.Features.Events.ListEvents;
using NSubstitute;

namespace EventosVivos.Application.Tests.Features.Events;

public class ListEventsHandlerTests
{
    [Fact]
    public async Task Delegates_to_the_reader_and_returns_its_result()
    {
        var reader = Substitute.For<IEventListReader>();
        var query = new ListEventsQuery(Title: "jazz", Page: 2, PageSize: 5);
        var expected = new PagedResult<EventListItem>([], Total: 0, Page: 2, PageSize: 5);
        reader.ListAsync(query, Arg.Any<CancellationToken>()).Returns(expected);
        var handler = new ListEventsHandler(reader);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Same(expected, result);
        await reader.Received(1).ListAsync(query, Arg.Any<CancellationToken>());
    }
}
