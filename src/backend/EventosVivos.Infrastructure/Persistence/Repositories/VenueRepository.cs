using EventosVivos.Domain.Venues;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence.Repositories;

internal sealed class VenueRepository(EventosVivosDbContext context) : IVenueRepository
{
    public Task<Venue?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Venues.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public void Touch(Venue venue) => context.Entry(venue).State = EntityState.Modified;
}
