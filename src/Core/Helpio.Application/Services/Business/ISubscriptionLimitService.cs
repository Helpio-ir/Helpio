using Helpio.Ir.Domain.Entities.Business;

namespace Helpio.Ir.Application.Services.Business
{
    public interface ISubscriptionLimitService
    {
        Task<bool> CanCreateTicketAsync(int organizationId);
        Task<int> GetRemainingTicketsAsync(int organizationId);
        Task<Subscription?> GetActiveSubscriptionAsync(int organizationId);
        Task IncrementTicketCountAsync(int organizationId);
        Task<SubscriptionLimitInfo> GetSubscriptionLimitInfoAsync(int organizationId);
    }
}