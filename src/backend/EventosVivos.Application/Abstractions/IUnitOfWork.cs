namespace EventosVivos.Application.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>Discards tracked changes so a failed operation can be retried from a clean state.</summary>
    void ClearTracking();
}
