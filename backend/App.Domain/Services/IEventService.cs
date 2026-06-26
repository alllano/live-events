using App.Common.DTOs.Events;
using App.Common.DTOs.Reports;

namespace App.Domain.Services;

public interface IEventService
{
    /// <summary>
    /// Creates a new event after validating venue capacity (BR-01), venue schedule overlap (BR-02),
    /// and the weekend night-time restriction (BR-03).
    /// </summary>
    Task<EventDetailResponse> CreateEventAsync(CreateEventRequest request);

    /// <summary>
    /// Retrieves a single event by id, resolving its effective status (BR-06) and available tickets.
    /// </summary>
    Task<EventDetailResponse> GetEventByIdAsync(int id);

    /// <summary>
    /// Retrieves events matching the given filters, resolving each event's effective status (BR-06).
    /// </summary>
    Task<List<EventListItemResponse>> GetEventsAsync(EventFilterRequest filter);

    /// <summary>
    /// Builds the occupancy report for a given event (FR-06): tickets sold, tickets available, occupancy
    /// percentage, total revenue, and the event's effective status (BR-06).
    /// </summary>
    Task<OccupancyReportResponse> GetOccupancyReportAsync(int eventId);
}
