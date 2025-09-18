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

    public class SubscriptionLimitInfo
    {
        public bool CanCreateTickets { get; set; }
        public int RemainingTickets { get; set; }
        public int MonthlyLimit { get; set; }
        public int CurrentMonthUsage { get; set; }
        public SubscriptionPlanType PlanType { get; set; }
        public DateTime CurrentMonthStartDate { get; set; }
        public bool IsFreemium { get; set; }
        public string? LimitationMessage { get; set; }
    }
}