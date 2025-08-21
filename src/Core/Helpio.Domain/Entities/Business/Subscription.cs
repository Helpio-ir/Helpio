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
        public int? OrganizationId { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Features { get; set; } // JSON string of features
        
        // Navigation properties
        public virtual Core.Organization? Organization { get; set; }
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}