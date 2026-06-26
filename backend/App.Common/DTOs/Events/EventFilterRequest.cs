namespace App.Common.DTOs.Events;

public class EventFilterRequest
{
    public int? EventTypeId { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
    public int? VenueId { get; set; }
    public int? EventStatusId { get; set; }
    public string? TitleSearch { get; set; }
}
