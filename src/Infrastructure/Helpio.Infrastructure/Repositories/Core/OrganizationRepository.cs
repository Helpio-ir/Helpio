using Microsoft.EntityFrameworkCore;
using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Domain.Interfaces.Repositories.Core;
using Helpio.Ir.Infrastructure.Data;

namespace Helpio.Ir.Infrastructure.Repositories.Core
{
    public class OrganizationRepository : Repository<Organization>, IOrganizationRepository
    {
        public OrganizationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Organization>> GetActiveOrganizationsAsync()
        {
            return await _dbSet.Where(o => o.IsActive && !o.IsDeleted).ToListAsync();
        }

        public async Task<Organization?> GetWithBranchesAsync(int organizationId)
        {
            return await _dbSet
                .Include(o => o.Branches)
                .FirstOrDefaultAsync(o => o.Id == organizationId);
        }

        public async Task<Organization?> GetWithTicketCategoriesAsync(int organizationId)
        {
            return await _dbSet
                .Include(o => o.TicketCategories)
                .FirstOrDefaultAsync(o => o.Id == organizationId);
        }
    }
}