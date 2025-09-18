using System;
using System.Collections.Generic;

namespace Helpio.Ir.Domain.Entities.Business
{
    public enum SubscriptionStatus
    {
        Active = 1,
        Inactive = 2,
        Cancelled = 3,
        Expired = 4,
        Suspended = 5
    }

    public enum SubscriptionPlanType
    {
        Freemium = 1,
        Basic = 2,
        Professional = 3,
        Enterprise = 4
    }

    public class Subscription : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "USD";
        public int BillingCycleDays { get; set; } = 30; // Monthly by default
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
        public SubscriptionPlanType PlanType { get; set; } = SubscriptionPlanType.Freemium;
        public int? OrganizationId { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Features { get; set; } // JSON string of features
        
        // Freemium limitations
        public int MonthlyTicketLimit { get; set; } = 50; // Default for freemium
        public int CurrentMonthTicketCount { get; set; } = 0;
        public DateTime CurrentMonthStartDate { get; set; } = DateTime.UtcNow.Date.AddDays(1 - DateTime.UtcNow.Day);
        
        // Navigation properties
        public virtual Core.Organization? Organization { get; set; }
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        
        // Helper methods
        public bool IsFreemium => PlanType == SubscriptionPlanType.Freemium;
        public bool HasReachedTicketLimit => IsFreemium && CurrentMonthTicketCount >= MonthlyTicketLimit;
        
        public void ResetMonthlyCounterIfNeeded()
        {
            var currentMonth = DateTime.UtcNow.Date.AddDays(1 - DateTime.UtcNow.Day);
            if (CurrentMonthStartDate.Month != currentMonth.Month || CurrentMonthStartDate.Year != currentMonth.Year)
            {
                CurrentMonthStartDate = currentMonth;
                CurrentMonthTicketCount = 0;
            }
        }
        
        public void IncrementTicketCount()
        {
            ResetMonthlyCounterIfNeeded();
            CurrentMonthTicketCount++;
        }
        
        public int GetRemainingTickets()
        {
            ResetMonthlyCounterIfNeeded();
            return IsFreemium ? Math.Max(0, MonthlyTicketLimit - CurrentMonthTicketCount) : int.MaxValue;
        }
    }
}