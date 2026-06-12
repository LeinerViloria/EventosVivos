using EventosVivos.Application.Features.Reports.OccupancyReport;
using NSubstitute;

namespace EventosVivos.Application.Tests.Features.Reports;

public class ExportOccupancyReportHandlerTests
{
    [Fact]
    public async Task Reads_every_event_and_returns_the_generated_pdf_bytes()
    {
        var reader = Substitute.For<IOccupancyReportReader>();
        var generator = Substitute.For<IOccupancyReportPdfGenerator>();
        var items = new List<OccupancyReportItem>();
        var expected = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF
        reader.GetAllAsync(Arg.Any<CancellationToken>()).Returns(items);
        generator.Generate(items).Returns(expected);
        var handler = new ExportOccupancyReportHandler(reader, generator);

        var result = await handler.Handle(new ExportOccupancyReportQuery(), CancellationToken.None);

        Assert.Same(expected, result);
        await reader.Received(1).GetAllAsync(Arg.Any<CancellationToken>());
        generator.Received(1).Generate(items);
    }
}
