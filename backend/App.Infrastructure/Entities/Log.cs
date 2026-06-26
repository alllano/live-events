namespace App.Infrastructure.Entities;

public class Log
{
    public int Id { get; set; }
    public int LogTypeId { get; set; }
    public DateTime CreatedDate { get; set; }
    public required string Error { get; set; }
    public required string StackTrace { get; set; }
    public required string Request { get; set; }

    public LogType LogType { get; set; } = null!;
}
