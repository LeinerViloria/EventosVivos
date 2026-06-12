using EventosVivos.Application.Security;
using EventosVivos.Domain.Users;

namespace EventosVivos.Application.Tests.Security;

public class RolePermissionsTests
{
    [Fact]
    public void All_lists_every_permission_including_reservations_read()
    {
        Assert.Contains(Permissions.ReservationsRead, Permissions.All);
        Assert.Contains(Permissions.ReservationsCreate, Permissions.All);
    }

    [Fact]
    public void Admin_can_manage_reservations_but_a_user_cannot_read_them()
    {
        Assert.Contains(Permissions.ReservationsRead, RolePermissions.Catalog[UserRole.Admin]);
        Assert.Contains(Permissions.ReservationsConfirm, RolePermissions.Catalog[UserRole.Admin]);
        Assert.DoesNotContain(Permissions.ReservationsRead, RolePermissions.Catalog[UserRole.User]);
    }
}
