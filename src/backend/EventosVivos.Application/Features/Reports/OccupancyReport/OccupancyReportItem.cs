using EventosVivos.Domain.Events;

namespace EventosVivos.Application.Features.Reports.OccupancyReport;

/// <summary>
/// Occupancy metrics for a single event (RF-06). <paramref name="SoldTickets"/> counts confirmed
/// reservations; <paramref name="AvailableTickets"/> is what is still purchasable; occupancy is the
/// share of capacity already held; revenue is price × sold.
/// </summary>
public sealed record OccupancyReportItem(
    Guid EventId,
    string EventTitle,
    int Capacity,
    int SoldTickets,
    int AvailableTickets,
    double OccupancyPercent,
    decimal Revenue,
    EventStatus Status);
