using Helpio.Ir.Domain.Entities.Business;
using Helpio.Ir.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Helpio.Ir.Infrastructure.Data.Seeders
{
    public class PlanSeeder
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PlanSeeder> _logger;

        public PlanSeeder(IServiceProvider serviceProvider, ILogger<PlanSeeder> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            await SeedDefaultPlansAsync(context);
        }

        private async Task SeedDefaultPlansAsync(IApplicationDbContext context)
        {
            try
            {
                // Check if plans already exist
                if (context.Plans.Any())
                {
                    _logger.LogInformation("Plans already exist, skipping seeding");
                    return;
                }

                var plans = new List<Plan>
                {
                    new Plan
                    {
                        Name = "فریمیوم",
                        Description = "پلن رایگان با محدودیت ۵۰ تیکت در ماه",
                        Type = PlanType.Freemium,
                        Price = 0,
                        Currency = "IRR",
                        BillingCycleDays = 30,
                        MonthlyTicketLimit = 50,
                        HasApiAccess = true,
                        HasPrioritySupport = false,
                        Has24x7Support = false,
                        HasCustomBranding = false,
                        HasAdvancedReporting = false,
                        HasCustomIntegrations = false,
                        HasKnowledgeBase = true,
                        HasEmailIntegration = true,
                        HasCannedResponses = false,
                        HasCSATSurveys = false,
                        DisplayOrder = 1,
                        IsActive = true,
                        IsRecommended = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Plan
                    {
                        Name = "بیسیک",
                        Description = "پلن پایه برای کسب‌وکارهای کوچک",
                        Type = PlanType.Basic,
                        Price = 99000,
                        Currency = "IRR",
                        BillingCycleDays = 30,
                        MonthlyTicketLimit = 200,
                        HasApiAccess = true,
                        HasPrioritySupport = false,
                        Has24x7Support = false,
                        HasCustomBranding = false,
                        HasAdvancedReporting = false,
                        HasCustomIntegrations = false,
                        HasKnowledgeBase = true,
                        HasEmailIntegration = true,
                        HasCannedResponses = true,
                        HasCSATSurveys = false,
                        DisplayOrder = 2,
                        IsActive = true,
                        IsRecommended = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Plan
                    {
                        Name = "حرفه‌ای",
                        Description = "پلن پیشرفته برای تیم‌های بزرگ",
                        Type = PlanType.Professional,
                        Price = 299000,
                        Currency = "IRR",
                        BillingCycleDays = 30,
                        MonthlyTicketLimit = 1000,
                        HasApiAccess = true,
                        HasPrioritySupport = true,
                        Has24x7Support = false,
                        HasCustomBranding = true,
                        HasAdvancedReporting = true,
                        HasCustomIntegrations = false,
                        HasKnowledgeBase = true,
                        HasEmailIntegration = true,
                        HasCannedResponses = true,
                        HasCSATSurveys = true,
                        DisplayOrder = 3,
                        IsActive = true,
                        IsRecommended = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Plan
                    {
                        Name = "سازمانی",
                        Description = "پلن کامل برای سازمان‌های بزرگ",
                        Type = PlanType.Enterprise,
                        Price = 999000,
                        Currency = "IRR",
                        BillingCycleDays = 30,
                        MonthlyTicketLimit = -1, // Unlimited
                        HasApiAccess = true,
                        HasPrioritySupport = true,
                        Has24x7Support = true,
                        HasCustomBranding = true,
                        HasAdvancedReporting = true,
                        HasCustomIntegrations = true,
                        HasKnowledgeBase = true,
                        HasEmailIntegration = true,
                        HasCannedResponses = true,
                        HasCSATSurveys = true,
                        DisplayOrder = 4,
                        IsActive = true,
                        IsRecommended = false,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                foreach (var plan in plans)
                {
                    context.Plans.Add(plan);
                }

                await context.SaveChangesAsync();

                _logger.LogInformation("Successfully seeded {PlanCount} default plans", plans.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while seeding plans");
            }
        }
    }
}