using App.Common.DTOs.Events;
using App.Infrastructure.Entities;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class EventRepository : IEventRepository
{
    private readonly AppDbContext _context;

    public EventRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Event?> GetByIdAsync(int id)
    {
        return await _context.Events
            .Include(eventEntity => eventEntity.Venue)
            .Include(eventEntity => eventEntity.EventType)
            .Include(eventEntity => eventEntity.EventStatus)
            .FirstOrDefaultAsync(eventEntity => eventEntity.Id == id);
    }

    public async Task<List<Event>> GetAllAsync(EventFilterRequest filter)
    {
        IQueryable<Event> query = _context.Events
            .Include(eventEntity => eventEntity.Venue)
            .Include(eventEntity => eventEntity.EventType)
            .Include(eventEntity => eventEntity.EventStatus);

        if (filter.EventTypeId.HasValue)
        {
            query = query.Where(eventEntity => eventEntity.EventTypeId == filter.EventTypeId.Value);
        }

        if (filter.StartDateFrom.HasValue)
        {
            query = query.Where(eventEntity => eventEntity.StartDate >= filter.StartDateFrom.Value);
        }

        if (filter.StartDateTo.HasValue)
        {
            query = query.Where(eventEntity => eventEntity.StartDate <= filter.StartDateTo.Value);
        }

        if (filter.VenueId.HasValue)
        {
            query = query.Where(eventEntity => eventEntity.VenueId == filter.VenueId.Value);
        }

        if (filter.EventStatusId.HasValue)
        {
            query = query.Where(eventEntity => eventEntity.EventStatusId == filter.EventStatusId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.TitleSearch))
        {
            string titleSearchLowered = filter.TitleSearch.ToLower();
            query = query.Where(eventEntity => eventEntity.Name.ToLower().Contains(titleSearchLowered));
        }

        return await query.ToListAsync();
    }

    public async Task AddAsync(Event newEvent)
    {
        await _context.Events.AddAsync(newEvent);
    }

    public async Task<int?> GetVenueCapacityAsync(int venueId)
    {
        return await _context.Venues
            .Where(venue => venue.Id == venueId)
            .Select(venue => (int?)venue.Capacity)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Event>> GetOverlappingEventsAsync(int venueId, DateTime startDate, DateTime endDate, int? excludeEventId)
    {
        IQueryable<Event> query = _context.Events
            .Where(eventEntity => eventEntity.VenueId == venueId)
            .Where(eventEntity => eventEntity.EventStatus.Name == "Active")
            .Where(eventEntity => eventEntity.StartDate < endDate && eventEntity.EndDate > startDate);

        if (excludeEventId.HasValue)
        {
            query = query.Where(eventEntity => eventEntity.Id != excludeEventId.Value);
        }

        return await query.ToListAsync();
    }
}
