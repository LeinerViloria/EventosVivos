namespace EventosVivos.Application.Security;

/// <summary>
/// Stable permission codes that gate the application's operations. Endpoints require one of
/// these through an authorization policy; the catalog maps each role to the codes it holds.
/// </summary>
public static class Permissions
{
    public const string EventsRead = "events.read";
    public const string EventsCreate = "events.create";
    public const string ReservationsCreate = "reservations.create";
    public const string ReservationsConfirm = "reservations.confirm";
    public const string ReservationsCancel = "reservations.cancel";
    public const string ReportsRead = "reports.read";
}
