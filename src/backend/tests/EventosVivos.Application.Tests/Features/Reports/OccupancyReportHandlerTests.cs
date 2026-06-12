using EventosVivos.Application.Abstractions;
using EventosVivos.Application.Features.Reports.OccupancyReport;
using NSubstitute;

namespace EventosVivos.Application.Tests.Features.Reports;

public class OccupancyReportHandlerTests
{
    [Fact]
    public async Task Delegates_to_the_reader_and_returns_its_result()
    {
        var reader = Substitute.For<IOccupancyReportReader>();
        var query = new OccupancyReportQuery(Page: 2, PageSize: 5);
        var expected = new PagedResult<OccupancyReportItem>([], Total: 0, Page: 2, PageSize: 5);
        reader.GetAsync(query, Arg.Any<CancellationToken>()).Returns(expected);
        var handler = new OccupancyReportHandler(reader);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Same(expected, result);
        await reader.Received(1).GetAsync(query, Arg.Any<CancellationToken>());
    }
}
