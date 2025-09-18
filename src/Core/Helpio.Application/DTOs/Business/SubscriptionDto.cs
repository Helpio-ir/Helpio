using System.ComponentModel.DataAnnotations;
using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.Application.DTOs.Core;
using Helpio.Ir.Domain.Entities.Business;

namespace Helpio.Ir.Application.DTOs.Business
{
    public enum SubscriptionStatusDto
    {
        Active = 1,
        Inactive = 2,
        Cancelled = 3,
        Expired = 4,
        Suspended = 5
    }

    public class SubscriptionDto : BaseDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public SubscriptionStatusDto Status { get; set; }
        public bool IsActive { get; set; }
        
        // Foreign Keys
        public int PlanId { get; set; }
        public int OrganizationId { get; set; }
        
        // Usage tracking
        public int CurrentPeriodTicketCount { get; set; }
        public DateTime CurrentPeriodStartDate { get; set; }
        
        // Custom overrides
        public int? CustomMonthlyTicketLimit { get; set; }
        public decimal? CustomPrice { get; set; }
        
        // Navigation DTOs
        public PlanDto? Plan { get; set; }
        public OrganizationDto? Organization { get; set; }
        
        // Computed Properties
        public bool IsExpired => EndDate.HasValue && EndDate < DateTime.UtcNow;
        public bool IsExpiringSoon => EndDate.HasValue && EndDate <= DateTime.UtcNow.AddDays(30);
        public int DaysRemaining => EndDate.HasValue ? Math.Max(0, (int)(EndDate.Value - DateTime.UtcNow).TotalDays) : int.MaxValue;
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
        
        public int GetRemainingTickets()
        {
            var limit = GetMonthlyTicketLimit();
            return limit == -1 ? int.MaxValue : Math.Max(0, limit - CurrentPeriodTicketCount);
        }
        
        public bool HasReachedTicketLimit()
        {
            var limit = GetMonthlyTicketLimit();
            return limit != -1 && CurrentPeriodTicketCount >= limit;
        }
    }

    public class CreateSubscriptionDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        
        public DateTime? EndDate { get; set; }
        
        [Required]
        public int PlanId { get; set; }
        
        [Required]
        public int OrganizationId { get; set; }
        
        // Optional custom overrides
        public int? CustomMonthlyTicketLimit { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal? CustomPrice { get; set; }
    }

    public class UpdateSubscriptionDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        public SubscriptionStatusDto Status { get; set; }
        
        public bool IsActive { get; set; }
        
        // Allow changing plan
        public int? PlanId { get; set; }
        
        // Optional custom overrides
        public int? CustomMonthlyTicketLimit { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal? CustomPrice { get; set; }
    }
}