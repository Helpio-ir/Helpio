using Helpio.Ir.Domain.Entities.Business;
using Helpio.Ir.Domain.Interfaces.Repositories;

namespace Helpio.Ir.Domain.Interfaces.Repositories.Business
{
    public interface ISubscriptionRepository : IRepository<Subscription>
    {
        Task<IEnumerable<Subscription>> GetByOrganizationIdAsync(int organizationId);
        Task<IEnumerable<Subscription>> GetByStatusAsync(SubscriptionStatus status);
        Task<IEnumerable<Subscription>> GetActiveSubscriptionsAsync();
        Task<IEnumerable<Subscription>> GetExpiringSubscriptionsAsync(DateTime expiryDate);
        Task<IEnumerable<Subscription>> GetExpiredSubscriptionsAsync();
    }
}