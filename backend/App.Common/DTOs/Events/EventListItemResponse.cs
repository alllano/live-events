namespace App.Common.DTOs.Events;

public class EventListItemResponse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string EventTypeName { get; set; }

    // Reflects the effective status (combines the persisted status with the EndDate < Now check), not only the persisted value.
    public required string EventStatusName { get; set; }

    public required string VenueName { get; set; }
    public DateTime StartDate { get; set; }
    public decimal Price { get; set; }
}
