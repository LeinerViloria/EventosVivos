using EventosVivos.Domain.Users;

namespace EventosVivos.Application.Security;

/// <summary>
/// The role-to-permissions catalog seeded into Redis. Admin can do everything; a regular user
/// can browse events and manage their own reservations.
/// </summary>
public static class RolePermissions
{
    public static IReadOnlyDictionary<UserRole, IReadOnlyList<string>> Catalog { get; } =
        new Dictionary<UserRole, IReadOnlyList<string>>
        {
            [UserRole.Admin] =
            [
                Permissions.EventsRead,
                Permissions.EventsCreate,
                Permissions.ReservationsRead,
                Permissions.ReservationsCreate,
                Permissions.ReservationsConfirm,
                Permissions.ReservationsCancel,
                Permissions.ReportsRead,
            ],
            [UserRole.User] =
            [
                Permissions.EventsRead,
                Permissions.ReservationsCreate,
                Permissions.ReservationsCancel,
            ],
        };
}
