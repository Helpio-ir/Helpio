using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Helpio.Ir.Application.Common.Interfaces;
using Helpio.Ir.Domain.Interfaces;
using Helpio.Ir.Domain.Entities.Business;

namespace Helpio.Ir.Application.Services.Business
{
    public class SubscriptionLimitService : ISubscriptionLimitService
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<SubscriptionLimitService> _logger;
        private readonly IDateTime _dateTime;

        public SubscriptionLimitService(
            IApplicationDbContext context,
            ILogger<SubscriptionLimitService> logger,
            IDateTime dateTime)
        {
            _context = context;
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

            subscription.ResetPeriodCounterIfNeeded();

            var canCreate = !subscription.HasReachedTicketLimit();
            
            if (!canCreate)
            {
                var limit = subscription.GetMonthlyTicketLimit();
                _logger.LogWarning("Ticket limit reached for organization {OrganizationId}. Current count: {Count}, Limit: {Limit}", 
                    organizationId, subscription.CurrentPeriodTicketCount, limit);
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
            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .Where(s => s.OrganizationId == organizationId && s.IsActive && s.Status == SubscriptionStatus.Active)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            return subscription;
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
            subscription.UpdatedAt = DateTime.UtcNow;
            
            _context.Update(subscription);
            await _context.SaveChangesAsync();

            var limit = subscription.GetMonthlyTicketLimit();
            _logger.LogInformation("Incremented ticket count for organization {OrganizationId}. New count: {Count}/{Limit}", 
                organizationId, subscription.CurrentPeriodTicketCount, limit);
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
                    PlanType = PlanType.Freemium,
                    CurrentMonthStartDate = _dateTime.UtcNow.Date.AddDays(1 - _dateTime.UtcNow.Day),
                    IsFreemium = true,
                    LimitationMessage = "هیچ اشتراک فعالی برای سازمان شما یافت نشد."
                };
            }

            subscription.ResetPeriodCounterIfNeeded();

            var remainingTickets = subscription.GetRemainingTickets();
            var canCreate = !subscription.HasReachedTicketLimit();
            var monthlyLimit = subscription.GetMonthlyTicketLimit();

            string? limitationMessage = null;
            if (subscription.IsFreemium)
            {
                if (remainingTickets <= 0)
                {
                    limitationMessage = $"شما به حد مجاز ماهانه {monthlyLimit} تیکت رسیده‌اید. لطفاً برای ایجاد تیکت بیشتر، اشتراک خود را ارتقا دهید.";
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
                MonthlyLimit = monthlyLimit,
                CurrentMonthUsage = subscription.CurrentPeriodTicketCount,
                PlanType = subscription.Plan?.Type ?? PlanType.Freemium,
                CurrentMonthStartDate = subscription.CurrentPeriodStartDate,
                IsFreemium = subscription.IsFreemium,
                LimitationMessage = limitationMessage
            };
        }
    }
}