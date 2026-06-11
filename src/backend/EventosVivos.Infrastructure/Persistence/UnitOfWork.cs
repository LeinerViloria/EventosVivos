using EventosVivos.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence;

internal sealed class UnitOfWork(EventosVivosDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ConcurrencyConflictException(exception);
        }
    }

    public void ClearTracking() => context.ChangeTracker.Clear();
}
