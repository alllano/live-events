namespace App.Infrastructure.Entities;

public class Venue
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int Capacity { get; set; }
    public int CityId { get; set; }

    public City City { get; set; } = null!;
    public ICollection<Event> Events { get; set; } = new List<Event>();
}
