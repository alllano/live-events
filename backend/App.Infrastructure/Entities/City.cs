namespace App.Infrastructure.Entities;

public class City
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public ICollection<Venue> Venues { get; set; } = new List<Venue>();
}
