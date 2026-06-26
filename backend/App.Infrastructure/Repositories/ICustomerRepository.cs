using App.Infrastructure.Entities;

namespace App.Infrastructure.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByEmailAsync(string email);
    Task AddAsync(Customer newCustomer);
}
