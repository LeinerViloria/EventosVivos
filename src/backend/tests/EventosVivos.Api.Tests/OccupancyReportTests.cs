using System.Net;
using System.Net.Http.Json;
using EventosVivos.Application.Features.Reports.OccupancyReport;
using EventosVivos.Domain.Events;
using EventosVivos.Infrastructure.Persistence.Configurations;
using Microsoft.Extensions.DependencyInjection;

namespace EventosVivos.Api.Tests;

public sealed class OccupancyReportTests(EventsApiFactory factory) : IClassFixture<EventsApiFactory>
{
    private sealed record CreatedEvent(Guid Id);

    private sealed record CreatedReservation(Guid Id);

    private sealed record ReportItem(
        Guid EventId,
        string EventTitle,
        int Capacity,
        int SoldTickets,
        int AvailableTickets,
        double OccupancyPercent,
        decimal Revenue,
        int Status);

    private sealed record PagedReport(List<ReportItem> Items, int Total);

    private static async Task<Guid> CreateEvent(HttpClient client, string title, int day)
    {
        var start = new DateTimeOffset(2026, 12, day, 18, 0, 0, TimeSpan.Zero);
        var request = new
        {
            title,
            description = "An event for occupancy report tests.",
            venueId = VenueIds.AuditorioCentral,
            maxCapacity = 100,
            startsAt = start,
            endsAt = start.AddHours(2),
            price = 50m,
            type = 1,
        };
        var response = await client.PostAsJsonAsync("/api/v1/events", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CreatedEvent>())!.Id;
    }

    private static async Task<Guid> Reserve(HttpClient client, Guid eventId, int quantity)
    {
        var request = new { eventId, buyerName = "Ana", buyerEmail = "ana@example.com", quantity };
        var response = await client.PostAsJsonAsync("/api/v1/reservations", request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CreatedReservation>())!.Id;
    }

    [Fact]
    public async Task Reports_sold_available_occupancy_and_revenue_for_a_confirmed_reservation()
    {
        var admin = await factory.CreateAdminClientAsync();
        var eventId = await CreateEvent(admin, "Reported Event", day: 10);
        var reservationId = await Reserve(admin, eventId, quantity: 2);
        await admin.PostAsync($"/api/v1/reservations/{reservationId}/confirm", content: null);

        var report = await admin.GetFromJsonAsync<PagedReport>("/api/v1/reports/occupancy?pageSize=100");
        var item = report!.Items.Single(i => i.EventId == eventId);

        Assert.Equal(100, item.Capacity);
        Assert.Equal(2, item.SoldTickets);
        Assert.Equal(98, item.AvailableTickets);
        Assert.Equal(2d, item.OccupancyPercent, 3);
        Assert.Equal(100m, item.Revenue); // price 50 × 2 confirmed
        Assert.Equal(1, item.Status); // EventStatus.Active
    }

    [Fact]
    public async Task A_pending_reservation_counts_as_occupied_but_not_sold()
    {
        var admin = await factory.CreateAdminClientAsync();
        var eventId = await CreateEvent(admin, "Pending Hold Event", day: 11);
        await Reserve(admin, eventId, quantity: 4); // held, not confirmed

        var report = await admin.GetFromJsonAsync<PagedReport>("/api/v1/reports/occupancy?pageSize=100");
        var item = report!.Items.Single(i => i.EventId == eventId);

        Assert.Equal(0, item.SoldTickets); // nothing confirmed
        Assert.Equal(96, item.AvailableTickets); // 4 held
        Assert.Equal(4d, item.OccupancyPercent, 3); // occupancy counts the hold
        Assert.Equal(0m, item.Revenue); // revenue only from confirmed
    }

    [Fact]
    public async Task A_regular_user_cannot_read_the_report()
    {
        var user = await factory.CreateUserClientAsync();

        var response = await user.GetAsync("/api/v1/reports/occupancy");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Exports_the_report_as_a_pdf()
    {
        var admin = await factory.CreateAdminClientAsync();
        await CreateEvent(admin, "Pdf Event", day: 12);

        var response = await admin.GetAsync("/api/v1/reports/occupancy/pdf");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType!.MediaType);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.StartsWith("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));
    }

    [Fact]
    public async Task A_regular_user_cannot_export_the_pdf()
    {
        var user = await factory.CreateUserClientAsync();

        var response = await user.GetAsync("/api/v1/reports/occupancy/pdf");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public void Pdf_generator_renders_a_valid_document_with_rows()
    {
        var generator = factory.Services.GetRequiredService<IOccupancyReportPdfGenerator>();
        var items = new List<OccupancyReportItem>
        {
            new(Guid.CreateVersion7(), "Concierto", 100, 30, 60, 40, 1500m, EventStatus.Active),
            new(Guid.CreateVersion7(), "Taller", 50, 0, 50, 0, 0m, EventStatus.Completed),
        };

        var pdf = generator.Generate(items);

        Assert.NotEmpty(pdf);
        Assert.StartsWith("%PDF", System.Text.Encoding.ASCII.GetString(pdf, 0, 4));
    }
}
