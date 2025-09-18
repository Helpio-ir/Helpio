using Microsoft.Extensions.Logging;
using Helpio.Ir.Application.Common.Interfaces;
using Helpio.Ir.Domain.Interfaces;
using Helpio.Ir.Domain.Entities.Business;

namespace Helpio.Ir.Application.Services.Business
{
    public class SubscriptionLimitService : ISubscriptionLimitService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SubscriptionLimitService> _logger;
        private readonly IDateTime _dateTime;

        public SubscriptionLimitService(
            IUnitOfWork unitOfWork,
            ILogger<SubscriptionLimitService> logger,
            IDateTime dateTime)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _dateTime = dateTime;
        }

        public async Task<bool> CanCreateTicketAsync(int organizationId)
        {
            var subscription = await GetActiveSubscriptionAsync(organizationId);
            
            if (subscription == null)
            {
                _logger.LogWarning("No active subscription found for organization {OrganizationId}", organizationId);
                return false;
            }

            subscription.ResetMonthlyCounterIfNeeded();

            var canCreate = !subscription.HasReachedTicketLimit;
            
            if (!canCreate)
            {
                _logger.LogWarning("Ticket limit reached for organization {OrganizationId}. Current count: {Count}, Limit: {Limit}", 
                    organizationId, subscription.CurrentMonthTicketCount, subscription.MonthlyTicketLimit);
            }

            return canCreate;
        }

        public async Task<int> GetRemainingTicketsAsync(int organizationId)
        {
            var subscription = await GetActiveSubscriptionAsync(organizationId);
            
            if (subscription == null)
            {
                return 0;
            }

            return subscription.GetRemainingTickets();
        }

        public async Task<Subscription?> GetActiveSubscriptionAsync(int organizationId)
        {
            var subscriptions = await _unitOfWork.Subscriptions.GetByOrganizationIdAsync(organizationId);
            
            var activeSubscription = subscriptions
                .Where(s => s.IsActive && s.Status == SubscriptionStatus.Active)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefault();

            return activeSubscription;
        }

        public async Task IncrementTicketCountAsync(int organizationId)
        {
            var subscription = await GetActiveSubscriptionAsync(organizationId);
            
            if (subscription == null)
            {
                _logger.LogWarning("No active subscription found for organization {OrganizationId} while incrementing ticket count", organizationId);
                return;
            }

            subscription.IncrementTicketCount();
            await _unitOfWork.Subscriptions.UpdateAsync(subscription);

            _logger.LogInformation("Incremented ticket count for organization {OrganizationId}. New count: {Count}/{Limit}", 
                organizationId, subscription.CurrentMonthTicketCount, subscription.MonthlyTicketLimit);
        }

        public async Task<SubscriptionLimitInfo> GetSubscriptionLimitInfoAsync(int organizationId)
        {
            var subscription = await GetActiveSubscriptionAsync(organizationId);
            
            if (subscription == null)
            {
                return new SubscriptionLimitInfo
                {
                    CanCreateTickets = false,
                    RemainingTickets = 0,
                    MonthlyLimit = 0,
                    CurrentMonthUsage = 0,
                    PlanType = SubscriptionPlanType.Freemium,
                    CurrentMonthStartDate = _dateTime.UtcNow.Date.AddDays(1 - _dateTime.UtcNow.Day),
                    IsFreemium = true,
                    LimitationMessage = "هیچ اشتراک فعالی برای سازمان شما یافت نشد."
                };
            }

            subscription.ResetMonthlyCounterIfNeeded();

            var remainingTickets = subscription.GetRemainingTickets();
            var canCreate = !subscription.HasReachedTicketLimit;

            string? limitationMessage = null;
            if (subscription.IsFreemium)
            {
                if (remainingTickets <= 0)
                {
                    limitationMessage = $"شما به حد مجاز ماهانه {subscription.MonthlyTicketLimit} تیکت رسیده‌اید. لطفاً برای ایجاد تیکت بیشتر، اشتراک خود را ارتقا دهید.";
                }
                else if (remainingTickets <= 5)
                {
                    limitationMessage = $"هشدار: شما تنها {remainingTickets} تیکت باقی‌مانده در این ماه دارید.";
                }
            }

            return new SubscriptionLimitInfo
            {
                CanCreateTickets = canCreate,
                RemainingTickets = remainingTickets,
                MonthlyLimit = subscription.MonthlyTicketLimit,
                CurrentMonthUsage = subscription.CurrentMonthTicketCount,
                PlanType = subscription.PlanType,
                CurrentMonthStartDate = subscription.CurrentMonthStartDate,
                IsFreemium = subscription.IsFreemium,
                LimitationMessage = limitationMessage
            };
        }
    }
}