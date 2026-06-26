namespace App.Common.DTOs.Reservations;

public class CreateReservationRequest
{
    public int EventId { get; set; }
    public int TicketQuantity { get; set; }
    public required string CustomerName { get; set; }
    public required string CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
}
