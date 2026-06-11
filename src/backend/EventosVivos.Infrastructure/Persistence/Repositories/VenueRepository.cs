using EventosVivos.Domain.Venues;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

internal sealed class VenueRepository(EventosVivosDbContext context) : IVenueRepository
{
    public Task<Venue?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Venues.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public void Touch(Venue venue) => context.Entry(venue).State = EntityState.Modified;

    public async Task<IReadOnlyList<Venue>> SearchAsync(
        string? term,
        int limit,
        CancellationToken cancellationToken)
    {
        var query = context.Venues.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(term))
        {
            query = query.Where(v => EF.Functions.ILike(v.Name, $"%{term}%"));
        }

        return await query
            .OrderBy(v => v.Name)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
