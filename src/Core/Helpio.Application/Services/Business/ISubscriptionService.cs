using Helpio.Ir.Application.DTOs.Business;
using Helpio.Ir.Application.DTOs.Common;

namespace Helpio.Ir.Application.Services.Business
{
    public interface ISubscriptionService
    {
        Task<SubscriptionDto?> GetByIdAsync(int id);
        Task<PaginatedResult<SubscriptionDto>> GetSubscriptionsAsync(PaginationRequest request);
        Task<SubscriptionDto> CreateAsync(CreateSubscriptionDto createDto);
        Task<SubscriptionDto> UpdateAsync(int id, UpdateSubscriptionDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<SubscriptionDto>> GetByOrganizationIdAsync(int organizationId);
        Task<IEnumerable<SubscriptionDto>> GetByStatusAsync(SubscriptionStatusDto status);
        Task<IEnumerable<SubscriptionDto>> GetActiveSubscriptionsAsync();
        Task<IEnumerable<SubscriptionDto>> GetExpiringSubscriptionsAsync(DateTime expiryDate);
        Task<IEnumerable<SubscriptionDto>> GetExpiredSubscriptionsAsync();
        Task<bool> ExtendSubscriptionAsync(int subscriptionId, DateTime newEndDate);
        Task<bool> CancelSubscriptionAsync(int subscriptionId);
        Task<bool> RenewSubscriptionAsync(int subscriptionId);
    }
}