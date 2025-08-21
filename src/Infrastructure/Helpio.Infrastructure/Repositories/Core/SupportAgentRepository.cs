using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Domain.Interfaces.Repositories.Core;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Ir.Infrastructure.Repositories.Core
{
    public class SupportAgentRepository : Repository<SupportAgent>, ISupportAgentRepository
    {
        public SupportAgentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<SupportAgent>> GetByTeamIdAsync(int teamId)
        {
            return await _dbSet
                .Where(sa => sa.TeamId == teamId && sa.IsActive && !sa.IsDeleted)
                .Include(sa => sa.User)
                .Include(sa => sa.Profile)
                .ToListAsync();
        }

        public async Task<IEnumerable<SupportAgent>> GetAvailableAgentsAsync()
        {
            return await _dbSet
                .Where(sa => sa.IsActive && sa.IsAvailable && !sa.IsDeleted)
                .Include(sa => sa.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<SupportAgent>> GetBySpecializationAsync(string specialization)
        {
            return await _dbSet
                .Where(sa => sa.Specialization == specialization && sa.IsActive && !sa.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<SupportAgent>> GetBySupportLevelAsync(int supportLevel)
        {
            return await _dbSet
                .Where(sa => sa.SupportLevel == supportLevel && sa.IsActive && !sa.IsDeleted)
                .ToListAsync();
        }

        public async Task<SupportAgent?> GetByAgentCodeAsync(string agentCode)
        {
            return await _dbSet
                .Include(sa => sa.User)
                .Include(sa => sa.Profile)
                .FirstOrDefaultAsync(sa => sa.AgentCode == agentCode);
        }

        public async Task<SupportAgent?> GetWithAssignedTicketsAsync(int agentId)
        {
            return await _dbSet
                .Include(sa => sa.AssignedTickets)
                .FirstOrDefaultAsync(sa => sa.Id == agentId);
        }

        public async Task<IEnumerable<SupportAgent>> GetAgentsWithLowWorkloadAsync(int maxTickets)
        {
            return await _dbSet
                .Where(sa => sa.CurrentTicketCount < maxTickets && sa.IsActive && sa.IsAvailable && !sa.IsDeleted)
                .OrderBy(sa => sa.CurrentTicketCount)
                .ToListAsync();
        }
    }
}