namespace App.Infrastructure.Entities;

public class Customer
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
