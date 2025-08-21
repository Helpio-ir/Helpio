using Helpio.Ir.Application.DTOs.Business;
using Helpio.Ir.Application.DTOs.Common;

namespace Helpio.Ir.Application.Services.Business
{
    public interface IOrderService
    {
        Task<OrderDto?> GetByIdAsync(int id);
        Task<OrderDto?> GetByOrderNumberAsync(string orderNumber);
        Task<PaginatedResult<OrderDto>> GetOrdersAsync(PaginationRequest request);
        Task<OrderDto> CreateAsync(CreateOrderDto createDto);
        Task<OrderDto> UpdateAsync(int id, UpdateOrderDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<OrderDto>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<OrderDto>> GetBySubscriptionIdAsync(int subscriptionId);
        Task<IEnumerable<OrderDto>> GetByStatusAsync(OrderStatusDto status);
        Task<IEnumerable<OrderDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalRevenueAsync();
        Task<IEnumerable<OrderDto>> GetPendingOrdersAsync();
        Task<bool> UpdateStatusAsync(int orderId, OrderStatusDto status);
        Task<Dictionary<string, decimal>> GetRevenueStatisticsAsync();
    }
}