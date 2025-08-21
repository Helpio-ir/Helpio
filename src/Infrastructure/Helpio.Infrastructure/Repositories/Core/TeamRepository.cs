using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Domain.Interfaces.Repositories.Core;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Ir.Infrastructure.Repositories.Core
{
    public class TeamRepository : Repository<Team>, ITeamRepository
    {
        public TeamRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Team>> GetByBranchIdAsync(int branchId)
        {
            return await _dbSet
                .Where(t => t.BranchId == branchId && !t.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Team>> GetActiveTeamsAsync()
        {
            return await _dbSet.Where(t => t.IsActive && !t.IsDeleted).ToListAsync();
        }

        public async Task<Team?> GetWithSupportAgentsAsync(int teamId)
        {
            return await _dbSet
                .Include(t => t.SupportAgents)
                .FirstOrDefaultAsync(t => t.Id == teamId);
        }

        public async Task<IEnumerable<Team>> GetTeamsByManagerAsync(int managerId)
        {
            return await _dbSet
                .Where(t => t.TeamLeadId == managerId || t.SupervisorId == managerId)
                .ToListAsync();
        }
    }
}