namespace App.Infrastructure.Entities;

public class Reservation
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int EventId { get; set; }
    public int ReservationStatusId { get; set; }
    public int TicketQuantity { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime ReservationDate { get; set; }
    public DateTime? CancelledDate { get; set; }
    public required string ReservationCode { get; set; }

    public Customer Customer { get; set; } = null!;
    public Event Event { get; set; } = null!;
    public ReservationStatus ReservationStatus { get; set; } = null!;
}
