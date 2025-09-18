using Helpio.Ir.Application.Common.Interfaces;
using Helpio.Ir.Domain.Entities.Business;
using Helpio.Ir.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Helpio.Ir.Application.Services.Business
{
    public class SubscriptionAnalyticsService : ISubscriptionAnalyticsService
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<SubscriptionAnalyticsService> _logger;

        public SubscriptionAnalyticsService(
            IApplicationDbContext context,
            ILogger<SubscriptionAnalyticsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<SubscriptionAnalytics> GetOrganizationAnalyticsAsync(int organizationId)
        {
            try
            {
                var subscription = await _context.Subscriptions
                    .Include(s => s.Plan)
                    .Where(s => s.OrganizationId == organizationId && s.IsActive)
                    .OrderByDescending(s => s.CreatedAt)
                    .FirstOrDefaultAsync();

                if (subscription == null)
                {
                    return new SubscriptionAnalytics
                    {
                        OrganizationId = organizationId,
                        CurrentPlanName = "بدون اشتراک",
                        CurrentPlanType = PlanType.Freemium,
                        HealthStatus = SubscriptionHealthStatus.Critical
                    };
                }

                var invoices = await _context.Invoices
                    .Where(i => i.OrganizationId == organizationId && i.Status == InvoiceStatus.Paid)
                    .ToListAsync();

                var monthlySpending = invoices
                    .Where(i => i.PaidDate >= DateTime.UtcNow.AddDays(-30))
                    .Sum(i => i.TotalAmount);

                var yearlySpending = invoices
                    .Where(i => i.PaidDate >= DateTime.UtcNow.AddDays(-365))
                    .Sum(i => i.TotalAmount);

                var daysRemaining = subscription.EndDate.HasValue 
                    ? Math.Max(0, (subscription.EndDate.Value - DateTime.UtcNow).Days)
                    : int.MaxValue;

                var healthStatus = CalculateHealthStatus(subscription, daysRemaining);

                return new SubscriptionAnalytics
                {
                    OrganizationId = organizationId,
                    CurrentPlanName = subscription.Plan?.Name ?? "نامشخص",
                    CurrentPlanType = subscription.Plan?.Type ?? PlanType.Freemium,
                    SubscriptionStartDate = subscription.StartDate,
                    SubscriptionEndDate = subscription.EndDate,
                    DaysRemaining = daysRemaining,
                    MonthlySpending = monthlySpending,
                    YearlySpending = yearlySpending,
                    IsInTrial = subscription.IsInTrial(),
                    HealthStatus = healthStatus
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting analytics for organization {OrganizationId}", organizationId);
                throw;
            }
        }

        public async Task<UsageAnalytics> GetUsageAnalyticsAsync(int organizationId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.Date.AddDays(1 - DateTime.UtcNow.Day);
                var end = endDate ?? start.AddMonths(1).AddDays(-1);

                var subscription = await _context.Subscriptions
                    .Include(s => s.Plan)
                    .Where(s => s.OrganizationId == organizationId && s.IsActive)
                    .FirstOrDefaultAsync();

                if (subscription == null)
                {
                    return new UsageAnalytics
                    {
                        OrganizationId = organizationId,
                        PeriodStart = start,
                        PeriodEnd = end,
                        MonthlyTicketLimit = 0
                    };
                }

                var totalTickets = await _context.Tickets
                    .Where(t => t.CreatedAt >= start && t.CreatedAt <= end)
                    .Where(t => t.Customer != null && _context.Customers
                        .Any(c => c.Id == t.CustomerId && c.OrganizationId == organizationId))
                    .CountAsync();

                var monthlyLimit = subscription.GetMonthlyTicketLimit();
                var usagePercentage = monthlyLimit > 0 ? (double)totalTickets / monthlyLimit * 100 : 0;

                return new UsageAnalytics
                {
                    OrganizationId = organizationId,
                    PeriodStart = start,
                    PeriodEnd = end,
                    TotalTicketsCreated = totalTickets,
                    MonthlyTicketLimit = monthlyLimit,
                    UsagePercentage = usagePercentage,
                    RemainingTickets = Math.Max(0, monthlyLimit - totalTickets),
                    DailyUsage = new List<DailyUsage>(),
                    UsageByCategory = new Dictionary<string, int>(),
                    UsageByPriority = new Dictionary<string, int>(),
                    PredictedMonthlyUsage = new PredictedUsage()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting usage analytics for organization {OrganizationId}", organizationId);
                throw;
            }
        }

        public async Task<PlanRecommendation> GetPlanRecommendationAsync(int organizationId)
        {
            try
            {
                var analytics = await GetUsageAnalyticsAsync(organizationId);
                var subscription = await _context.Subscriptions
                    .Include(s => s.Plan)
                    .Where(s => s.OrganizationId == organizationId && s.IsActive)
                    .FirstOrDefaultAsync();

                if (subscription?.Plan == null)
                {
                    return new PlanRecommendation
                    {
                        ShouldUpgrade = true,
                        RecommendedPlan = PlanType.Basic,
                        RecommendationReason = "برای شروع استفاده بهتر از سیستم، پلن بیسیک پیشنهاد می‌شود"
                    };
                }

                var currentPlan = subscription.Plan;
                var usageRate = analytics.UsagePercentage / 100.0;

                // اگر استفاده بالای 80% باشد، ارتقا پیشنهاد می‌دهیم
                if (usageRate >= 0.8 && currentPlan.Type != PlanType.Enterprise)
                {
                    var nextPlan = GetNextPlanType(currentPlan.Type);
                    return new PlanRecommendation
                    {
                        ShouldUpgrade = true,
                        RecommendedPlan = nextPlan,
                        RecommendationReason = $"استفاده شما {analytics.UsagePercentage:F1}% رسیده است. برای جلوگیری از محدودیت، ارتقا پیشنهاد می‌شود.",
                        Benefits = GetPlanBenefits(nextPlan)
                    };
                }

                return new PlanRecommendation
                {
                    RecommendationReason = "پلن فعلی برای استفاده شما مناسب است",
                    Benefits = new List<string> { "استفاده متعادل از امکانات پلن" }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting plan recommendation for organization {OrganizationId}", organizationId);
                throw;
            }
        }

        public async Task<SubscriptionInsights> GetSubscriptionInsightsAsync(int organizationId)
        {
            try
            {
                var analytics = await GetUsageAnalyticsAsync(organizationId);
                var insights = new SubscriptionInsights();

                // تحلیل الگوهای استفاده
                if (analytics.UsagePercentage > 90)
                {
                    insights.Warnings.Add("نزدیک به حد مجاز ماهانه هستید");
                }

                // محاسبه metrics کارایی (ساده‌سازی شده)
                insights.Efficiency = new EfficiencyMetrics
                {
                    TicketResolutionRate = 0.85,
                    AverageResponseTime = TimeSpan.FromHours(4),
                    CustomerSatisfactionScore = 4.2,
                    ActiveAgents = 3,
                    AgentProductivity = 0.75
                };

                return insights;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription insights for organization {OrganizationId}", organizationId);
                throw;
            }
        }

        public async Task TrackUsageEventAsync(int organizationId, string eventType, Dictionary<string, object>? metadata = null)
        {
            try
            {
                _logger.LogInformation("Tracking usage event {EventType} for organization {OrganizationId}", 
                    eventType, organizationId);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking usage event {EventType} for organization {OrganizationId}", 
                    eventType, organizationId);
            }
        }

        private SubscriptionHealthStatus CalculateHealthStatus(Subscription subscription, int daysRemaining)
        {
            if (subscription.IsExpired())
                return SubscriptionHealthStatus.Expired;

            if (daysRemaining <= 7)
                return SubscriptionHealthStatus.Critical;

            if (daysRemaining <= 30)
                return SubscriptionHealthStatus.Warning;

            return SubscriptionHealthStatus.Excellent;
        }

        private PlanType GetNextPlanType(PlanType currentType)
        {
            return currentType switch
            {
                PlanType.Freemium => PlanType.Basic,
                PlanType.Basic => PlanType.Professional,
                PlanType.Professional => PlanType.Enterprise,
                _ => PlanType.Enterprise
            };
        }

        private List<string> GetPlanBenefits(PlanType planType)
        {
            return planType switch
            {
                PlanType.Basic => new List<string> { "200 تیکت در ماه", "پاسخ‌های آماده" },
                PlanType.Professional => new List<string> { "1000 تیکت در ماه", "گزارش‌های پیشرفته", "برندینگ اختصاصی", "پشتیبانی اولویت‌دار" },
                PlanType.Enterprise => new List<string> { "تیکت نامحدود", "پشتیبانی 24/7", "یکپارچه‌سازی سفارشی" },
                _ => new List<string>()
            };
        }
    }
}