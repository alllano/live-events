namespace App.Infrastructure.Persistence;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();
}
