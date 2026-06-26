namespace App.Infrastructure.Entities;

public class LogType
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public ICollection<Log> Logs { get; set; } = new List<Log>();
}
