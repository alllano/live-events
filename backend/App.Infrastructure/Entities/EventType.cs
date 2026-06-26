namespace App.Infrastructure.Entities;

public class EventType
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
