using App.Infrastructure.Entities;

namespace App.Infrastructure.Repositories;

public interface IVenueRepository
{
    Task<Venue?> GetByIdAsync(int id);
}
