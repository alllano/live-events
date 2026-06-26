namespace App.Common.DTOs.Events;

public class CreateEventRequest
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public int VenueId { get; set; }
    public int MaxCapacity { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Price { get; set; }
    public int EventTypeId { get; set; }
}
