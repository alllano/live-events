using App.Infrastructure.Entities;
using App.Infrastructure.Persistence;

namespace App.Infrastructure.Repositories;

public class LogRepository : ILogRepository
{
    private readonly AppDbContext _context;

    public LogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Log newLog)
    {
        await _context.Logs.AddAsync(newLog);
    }
}
