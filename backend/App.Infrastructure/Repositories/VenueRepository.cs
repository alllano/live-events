using App.Infrastructure.Entities;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class VenueRepository : IVenueRepository
{
    private readonly AppDbContext _context;

    public VenueRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Venue?> GetByIdAsync(int id)
    {
        return await _context.Venues.FirstOrDefaultAsync(venue => venue.Id == id);
    }
}
