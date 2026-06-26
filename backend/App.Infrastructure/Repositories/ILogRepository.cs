using App.Infrastructure.Entities;

namespace App.Infrastructure.Repositories;

public interface ILogRepository
{
    Task AddAsync(Log newLog);
}
