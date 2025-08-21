using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Domain.Interfaces.Repositories.Core;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Ir.Infrastructure.Repositories.Core
{
    public class ProfileRepository : Repository<Profile>, IProfileRepository
    {
        public ProfileRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Profile?> GetBySupportAgentIdAsync(int supportAgentId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.SupportAgent != null && p.SupportAgent.Id == supportAgentId);
        }

        public async Task<IEnumerable<Profile>> GetBySkillsAsync(string skills)
        {
            return await _dbSet
                .Where(p => p.Skills.Contains(skills))
                .ToListAsync();
        }
    }
}