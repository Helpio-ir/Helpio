using Helpio.Ir.Domain.Entities.Business;
using Helpio.Ir.Domain.Interfaces.Repositories;

namespace Helpio.Ir.Domain.Interfaces.Repositories.Business
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<Order>> GetBySubscriptionIdAsync(int subscriptionId);
        Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status);
        Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Order?> GetByOrderNumberAsync(string orderNumber);
        Task<decimal> GetTotalRevenueAsync();
        Task<IEnumerable<Order>> GetPendingOrdersAsync();
    }
}