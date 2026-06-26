namespace App.Common.DTOs.Reservations;

public class ReservationResponse
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public required string EventName { get; set; }
    public required string CustomerName { get; set; }
    public int TicketQuantity { get; set; }
    public decimal TotalPrice { get; set; }
    public required string ReservationStatusName { get; set; }
    public required string ReservationCode { get; set; }
    public DateTime ReservationDate { get; set; }
    public DateTime? CancelledDate { get; set; }
}
