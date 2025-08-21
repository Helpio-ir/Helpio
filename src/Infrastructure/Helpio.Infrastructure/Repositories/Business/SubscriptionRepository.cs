using Helpio.Ir.Domain.Entities.Business;
using Helpio.Ir.Domain.Interfaces.Repositories.Business;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Ir.Infrastructure.Repositories.Business
{
    public class SubscriptionRepository : Repository<Subscription>, ISubscriptionRepository
    {
        public SubscriptionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Subscription>> GetByOrganizationIdAsync(int organizationId)
        {
            return await _dbSet
                .Where(s => s.OrganizationId == organizationId && !s.IsDeleted)
                .OrderByDescending(s => s.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Subscription>> GetByStatusAsync(SubscriptionStatus status)
        {
            return await _dbSet
                .Where(s => s.Status == status && !s.IsDeleted)
                .Include(s => s.Organization)
                .ToListAsync();
        }

        public async Task<IEnumerable<Subscription>> GetActiveSubscriptionsAsync()
        {
            return await _dbSet
                .Where(s => s.Status == SubscriptionStatus.Active && s.IsActive && !s.IsDeleted)
                .Include(s => s.Organization)
                .ToListAsync();
        }

        public async Task<IEnumerable<Subscription>> GetExpiringSubscriptionsAsync(DateTime expiryDate)
        {
            return await _dbSet
                .Where(s => s.EndDate.HasValue && s.EndDate <= expiryDate &&
                           s.Status == SubscriptionStatus.Active && !s.IsDeleted)
                .Include(s => s.Organization)
                .OrderBy(s => s.EndDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Subscription>> GetExpiredSubscriptionsAsync()
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Where(s => s.EndDate.HasValue && s.EndDate < now &&
                           s.Status != SubscriptionStatus.Expired && !s.IsDeleted)
                .Include(s => s.Organization)
                .ToListAsync();
        }
    }
}