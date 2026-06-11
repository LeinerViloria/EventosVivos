using EventosVivos.Domain.Reservations;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

internal sealed class ReservationRepository(EventosVivosDbContext context) : IReservationRepository
{
    public void Add(Reservation reservation) => context.Reservations.Add(reservation);
}
