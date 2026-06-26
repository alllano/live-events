namespace App.Infrastructure.Entities;

public class Event
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int EventTypeId { get; set; }
    public int EventStatusId { get; set; }
    public int VenueId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int MaxCapacity { get; set; }
    public decimal Price { get; set; }
    public required string Description { get; set; }

    public EventType EventType { get; set; } = null!;
    public EventStatus EventStatus { get; set; } = null!;
    public Venue Venue { get; set; } = null!;
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
