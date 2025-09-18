using Helpio.Ir.Domain.Entities.Business;

namespace Helpio.Ir.Application.Services.Business
{
    public interface ISubscriptionAnalyticsService
    {
        Task<SubscriptionAnalytics> GetOrganizationAnalyticsAsync(int organizationId);
        Task<UsageAnalytics> GetUsageAnalyticsAsync(int organizationId, DateTime? startDate = null, DateTime? endDate = null);
        Task<PlanRecommendation> GetPlanRecommendationAsync(int organizationId);
        Task<SubscriptionInsights> GetSubscriptionInsightsAsync(int organizationId);
        Task TrackUsageEventAsync(int organizationId, string eventType, Dictionary<string, object>? metadata = null);
    }

    public class SubscriptionAnalytics
    {
        public int OrganizationId { get; set; }
        public string CurrentPlanName { get; set; } = string.Empty;
        public PlanType CurrentPlanType { get; set; }
        public DateTime SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        public int DaysRemaining { get; set; }
        public decimal MonthlySpending { get; set; }
        public decimal YearlySpending { get; set; }
        public bool IsInTrial { get; set; }
        public SubscriptionHealthStatus HealthStatus { get; set; }
    }

    public class UsageAnalytics
    {
        public int OrganizationId { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int TotalTicketsCreated { get; set; }
        public int MonthlyTicketLimit { get; set; }
        public double UsagePercentage { get; set; }
        public int RemainingTickets { get; set; }
        public List<DailyUsage> DailyUsage { get; set; } = new();
        public Dictionary<string, int> UsageByCategory { get; set; } = new();
        public Dictionary<string, int> UsageByPriority { get; set; } = new();
        public PredictedUsage PredictedMonthlyUsage { get; set; } = new();
    }

    public class DailyUsage
    {
        public DateTime Date { get; set; }
        public int TicketCount { get; set; }
        public int ResponseCount { get; set; }
        public TimeSpan AverageResponseTime { get; set; }
    }

    public class PredictedUsage
    {
        public int PredictedTickets { get; set; }
        public int PredictedMonthlyTickets { get; set; }
        public decimal PredictedCost { get; set; }
        public DateTime PredictionDate { get; set; }
        public double Confidence { get; set; }
        public string PredictionMethod { get; set; } = "Linear";
        public bool WillExceedLimit { get; set; }
        public DateTime? PredictedLimitDate { get; set; }
    }

    public class PlanRecommendation
    {
        public bool ShouldUpgrade { get; set; }
        public bool ShouldDowngrade { get; set; }
        public PlanType? RecommendedPlan { get; set; }
        public string RecommendationReason { get; set; } = string.Empty;
        public decimal PotentialSavings { get; set; }
        public decimal PotentialCost { get; set; }
        public List<string> Benefits { get; set; } = new();
    }

    public class SubscriptionInsights
    {
        public List<string> Insights { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<string> Suggestions { get; set; } = new();
        public EfficiencyMetrics Efficiency { get; set; } = new();
    }

    public class EfficiencyMetrics
    {
        public double TicketResolutionRate { get; set; }
        public TimeSpan AverageResponseTime { get; set; }
        public double CustomerSatisfactionScore { get; set; }
        public int ActiveAgents { get; set; }
        public double AgentProductivity { get; set; }
        public DateTime LastCalculated { get; set; } = DateTime.UtcNow;
    }

    public class SubscriptionLimitInfo
    {
        public int CurrentMonthUsage { get; set; }
        public int MonthlyLimit { get; set; }
        public int RemainingTickets { get; set; }
        public bool CanCreateTickets { get; set; }
        public bool IsFreemium { get; set; }
        public PlanType PlanType { get; set; }
        public string? LimitationMessage { get; set; }
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public DateTime CurrentMonthStartDate { get; set; }
    }

    public enum SubscriptionHealthStatus
    {
        Excellent = 1,
        Good = 2,
        Warning = 3,
        Critical = 4,
        Expired = 5
    }
}