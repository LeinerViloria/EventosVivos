namespace EventosVivos.Domain.Reservations;

public interface IReservationRepository
{
    void Add(Reservation reservation);

    /// <summary>Loads a tracked reservation so state changes are saved.</summary>
    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> CodeExistsAsync(string confirmationCode, CancellationToken cancellationToken);
}
