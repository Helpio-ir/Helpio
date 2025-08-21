using Helpio.Ir.Domain.Entities.Ticketing;
using Helpio.Ir.Domain.Interfaces.Repositories.Ticketing;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Ir.Infrastructure.Repositories.Ticketing
{
    public class TicketCategoryRepository : Repository<TicketCategory>, ITicketCategoryRepository
    {
        public TicketCategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TicketCategory>> GetByOrganizationIdAsync(int organizationId)
        {
            return await _dbSet
                .Where(tc => tc.OrganizationId == organizationId && !tc.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<TicketCategory>> GetActiveCategoriesAsync()
        {
            return await _dbSet
                .Where(tc => tc.IsActive && !tc.IsDeleted)
                .ToListAsync();
        }
    }
}