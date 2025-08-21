using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Domain.Interfaces.Repositories.Core;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Ir.Infrastructure.Repositories.Core
{
    public class BranchRepository : Repository<Branch>, IBranchRepository
    {
        public BranchRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Branch>> GetByOrganizationIdAsync(int organizationId)
        {
            return await _dbSet
                .Where(b => b.OrganizationId == organizationId && !b.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Branch>> GetActiveBranchesAsync()
        {
            return await _dbSet.Where(b => b.IsActive && !b.IsDeleted).ToListAsync();
        }

        public async Task<Branch?> GetWithTeamsAsync(int branchId)
        {
            return await _dbSet
                .Include(b => b.Teams)
                .FirstOrDefaultAsync(b => b.Id == branchId);
        }
    }
}