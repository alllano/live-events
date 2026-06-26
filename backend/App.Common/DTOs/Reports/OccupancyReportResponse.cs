namespace App.Common.DTOs.Reports;

public class OccupancyReportResponse
{
    public int EventId { get; set; }
    public required string EventName { get; set; }
    public int TicketsSold { get; set; }
    public int TicketsAvailable { get; set; }
    public decimal OccupancyPercentage { get; set; }
    public decimal TotalRevenue { get; set; }
    public required string EventStatusName { get; set; }
}
