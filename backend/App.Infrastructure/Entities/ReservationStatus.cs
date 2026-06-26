namespace App.Infrastructure.Entities;

public class ReservationStatus
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
