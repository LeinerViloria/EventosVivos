namespace EventosVivos.Application.Security;

/// <summary>
/// Stable permission codes that gate the application's operations. Endpoints require one of
/// these through an authorization policy; the catalog maps each role to the codes it holds.
/// </summary>
public static class Permissions
{
    public const string EventsRead = "events.read";
    public const string EventsCreate = "events.create";
    public const string ReservationsRead = "reservations.read";
    public const string ReservationsCreate = "reservations.create";
    public const string ReservationsConfirm = "reservations.confirm";
    public const string ReservationsCancel = "reservations.cancel";
    public const string ReportsRead = "reports.read";

    /// <summary>Every permission code; used to register one authorization policy per permission.</summary>
    public static IReadOnlyList<string> All { get; } =
    [
        EventsRead,
        EventsCreate,
        ReservationsRead,
        ReservationsCreate,
        ReservationsConfirm,
        ReservationsCancel,
        ReportsRead,
    ];
}
