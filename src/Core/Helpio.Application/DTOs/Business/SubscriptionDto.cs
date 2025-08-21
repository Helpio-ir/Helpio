using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.Application.DTOs.Core;

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
        public decimal Price { get; set; }
        public string Currency { get; set; } = "USD";
        public int BillingCycleDays { get; set; }
        public SubscriptionStatusDto Status { get; set; }
        public int? OrganizationId { get; set; }
        public bool IsActive { get; set; }
        public string? Features { get; set; }
        
        // Navigation DTOs
        public OrganizationDto? Organization { get; set; }
        
        // Computed Properties
        public bool IsExpired => EndDate.HasValue && EndDate < DateTime.UtcNow;
        public bool IsExpiringSoon => EndDate.HasValue && EndDate <= DateTime.UtcNow.AddDays(30);
        public int DaysRemaining => EndDate.HasValue ? Math.Max(0, (int)(EndDate.Value - DateTime.UtcNow).TotalDays) : 0;
        public decimal MonthlyPrice => BillingCycleDays > 0 ? (Price / BillingCycleDays) * 30 : Price;
    }

    public class CreateSubscriptionDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "USD";
        public int BillingCycleDays { get; set; } = 30;
        public int? OrganizationId { get; set; }
        public string? Features { get; set; }
    }

    public class UpdateSubscriptionDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Price { get; set; }
        public int BillingCycleDays { get; set; }
        public SubscriptionStatusDto Status { get; set; }
        public bool IsActive { get; set; }
        public string? Features { get; set; }
    }
}