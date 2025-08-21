using Microsoft.EntityFrameworkCore;
using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Domain.Interfaces.Repositories.Core;
using Helpio.Ir.Infrastructure.Data;

namespace Helpio.Ir.Infrastructure.Repositories.Core
{
    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        public CustomerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<Customer?> GetByPhoneNumberAsync(string phoneNumber)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);
        }

        public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm)
        {
            return await _dbSet
                .Where(c => c.FirstName.Contains(searchTerm) || 
                           c.LastName.Contains(searchTerm) || 
                           c.Email.Contains(searchTerm) ||
                           c.CompanyName.Contains(searchTerm))
                .ToListAsync();
        }

        public async Task<IEnumerable<Customer>> GetCustomersWithTicketsAsync()
        {
            return await _dbSet
                .Include(c => c.Tickets)
                .Where(c => c.Tickets.Any())
                .ToListAsync();
        }
    }
}