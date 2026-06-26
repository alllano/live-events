using App.Common.DTOs.Events;
using App.Infrastructure.Entities;

namespace App.Infrastructure.Repositories;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(int id);
    Task<List<Event>> GetAllAsync(EventFilterRequest filter);
    Task AddAsync(Event newEvent);
    Task<int?> GetVenueCapacityAsync(int venueId);
    Task<List<Event>> GetOverlappingEventsAsync(int venueId, DateTime startDate, DateTime endDate, int? excludeEventId);
}
