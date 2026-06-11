namespace EventosVivos.Infrastructure.Messaging;

/// <summary>Stable type names that travel with each integration event through the bus.</summary>
public static class IntegrationEventTypes
{
    public const string TicketsReleased = "tickets-released";
}

/// <summary>Tickets returned to availability (e.g. an expired reservation). Drives the SSE update.</summary>
public sealed record TicketsReleasedIntegrationEvent(Guid EventId, int AvailableTickets);
