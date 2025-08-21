using Microsoft.EntityFrameworkCore;
using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Domain.Interfaces.Repositories.Core;
using Helpio.Ir.Infrastructure.Data;

namespace Helpio.Ir.Infrastructure.Repositories.Core
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByPhoneNumberAsync(string phoneNumber)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _dbSet.Where(u => u.IsActive && !u.IsDeleted).ToListAsync();
        }
    }
}