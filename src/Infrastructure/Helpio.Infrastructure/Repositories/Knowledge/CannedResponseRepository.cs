using Helpio.Ir.Domain.Entities.Knowledge;
using Helpio.Ir.Domain.Interfaces.Repositories.Knowledge;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Ir.Infrastructure.Repositories.Knowledge
{
    public class CannedResponseRepository : Repository<CannedResponse>, ICannedResponseRepository
    {
        public CannedResponseRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<CannedResponse>> GetByOrganizationIdAsync(int organizationId)
        {
            return await _dbSet
                .Where(cr => cr.OrganizationId == organizationId && !cr.IsDeleted)
                .OrderBy(cr => cr.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<CannedResponse>> GetActiveResponsesAsync()
        {
            return await _dbSet
                .Where(cr => cr.IsActive && !cr.IsDeleted)
                .OrderBy(cr => cr.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<CannedResponse>> SearchByTagsAsync(string tags)
        {
            return await _dbSet
                .Where(cr => cr.Tags != null && cr.Tags.Contains(tags) && cr.IsActive && !cr.IsDeleted)
                .OrderBy(cr => cr.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<CannedResponse>> GetMostUsedResponsesAsync(int count)
        {
            return await _dbSet
                .Where(cr => cr.IsActive && !cr.IsDeleted)
                .OrderByDescending(cr => cr.UsageCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<CannedResponse?> GetByNameAsync(string name)
        {
            return await _dbSet
                .FirstOrDefaultAsync(cr => cr.Name == name && !cr.IsDeleted);
        }
    }
}