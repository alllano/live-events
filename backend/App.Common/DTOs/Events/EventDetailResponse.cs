namespace App.Common.DTOs.Events;

public class EventDetailResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public int EventTypeId { get; set; }
    public required string EventTypeName { get; set; }
    public int EventStatusId { get; set; }

    // Reflects the effective status (combines the persisted status with the EndDate < Now check), not only the persisted value.
    public required string EventStatusName { get; set; }

    public int VenueId { get; set; }
    public required string VenueName { get; set; }
    public int MaxCapacity { get; set; }

    // Calculated as MaxCapacity minus tickets sold, blocked and lost — never persisted.
    public int AvailableTickets { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Price { get; set; }
}
