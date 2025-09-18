using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

    public class Subscription : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public DateTime StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;
        
        public bool IsActive { get; set; } = true;
        
        // Foreign Keys
        [Required]
        public int PlanId { get; set; }
        
        [Required]
        public int OrganizationId { get; set; }
        
        // Usage tracking for current billing period
        public int CurrentPeriodTicketCount { get; set; } = 0;
        
        public DateTime CurrentPeriodStartDate { get; set; } = DateTime.UtcNow.Date.AddDays(1 - DateTime.UtcNow.Day);
        
        // Custom overrides (optional - if organization needs custom limits)
        public int? CustomMonthlyTicketLimit { get; set; }
        
        public decimal? CustomPrice { get; set; }
        
        // Navigation properties
        public virtual Plan Plan { get; set; } = null!;
        
        public virtual Core.Organization Organization { get; set; } = null!;
        
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        
        // Helper methods
        public bool IsFreemium => Plan?.IsFreemium ?? false;
        
        public int GetMonthlyTicketLimit()
        {
            return CustomMonthlyTicketLimit ?? Plan?.MonthlyTicketLimit ?? 50;
        }
        
        public decimal GetPrice()
        {
            return CustomPrice ?? Plan?.Price ?? 0;
        }
        
        public string GetCurrency()
        {
            return Plan?.Currency ?? "IRR";
        }
        
        public bool HasReachedTicketLimit()
        {
            var limit = GetMonthlyTicketLimit();
            return limit != -1 && CurrentPeriodTicketCount >= limit;
        }
        
        public void ResetPeriodCounterIfNeeded()
        {
            var currentPeriodStart = DateTime.UtcNow.Date.AddDays(1 - DateTime.UtcNow.Day);
            if (CurrentPeriodStartDate.Month != currentPeriodStart.Month || 
                CurrentPeriodStartDate.Year != currentPeriodStart.Year)
            {
                CurrentPeriodStartDate = currentPeriodStart;
                CurrentPeriodTicketCount = 0;
            }
        }
        
        public void IncrementTicketCount()
        {
            ResetPeriodCounterIfNeeded();
            CurrentPeriodTicketCount++;
        }
        
        public int GetRemainingTickets()
        {
            ResetPeriodCounterIfNeeded();
            var limit = GetMonthlyTicketLimit();
            return limit == -1 ? int.MaxValue : Math.Max(0, limit - CurrentPeriodTicketCount);
        }
        
        public bool IsExpired()
        {
            return EndDate.HasValue && EndDate.Value < DateTime.UtcNow;
        }
        
        public bool IsInTrial()
        {
            return EndDate.HasValue && (EndDate.Value - StartDate).TotalDays <= 30;
        }
        
        public int GetDaysUntilExpiry()
        {
            if (!EndDate.HasValue) return int.MaxValue;
            return Math.Max(0, (int)(EndDate.Value - DateTime.UtcNow).TotalDays);
        }
    }
}