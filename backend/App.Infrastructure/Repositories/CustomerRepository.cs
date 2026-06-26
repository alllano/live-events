using App.Infrastructure.Entities;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByEmailAsync(string email)
    {
        string emailLowered = email.ToLower();

        return await _context.Customers
            .FirstOrDefaultAsync(customer => customer.Email.ToLower() == emailLowered);
    }

    public async Task AddAsync(Customer newCustomer)
    {
        await _context.Customers.AddAsync(newCustomer);
    }
}
