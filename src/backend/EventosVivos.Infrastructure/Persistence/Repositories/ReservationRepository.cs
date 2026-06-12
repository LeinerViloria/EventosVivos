using EventosVivos.Domain.Reservations;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

internal sealed class ReservationRepository(EventosVivosDbContext context) : IReservationRepository
{
    public void Add(Reservation reservation) => context.Reservations.Add(reservation);

    public Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Reservations.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<bool> CodeExistsAsync(string confirmationCode, CancellationToken cancellationToken) =>
        context.Reservations.AnyAsync(r => r.ConfirmationCode == confirmationCode, cancellationToken);
}
