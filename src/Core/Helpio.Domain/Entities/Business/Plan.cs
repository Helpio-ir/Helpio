using System.ComponentModel.DataAnnotations;

namespace Helpio.Ir.Domain.Entities.Business
{
    public enum PlanType
    {
        Freemium = 1,
        Basic = 2,
        Professional = 3,
        Enterprise = 4
    }

    public class Plan : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        [Required]
        public PlanType Type { get; set; }
        
        public decimal Price { get; set; }
        
        [MaxLength(3)]
        public string Currency { get; set; } = "IRR";
        
        public int BillingCycleDays { get; set; } = 30; // Monthly by default
        
        // Limits and Features
        public int MonthlyTicketLimit { get; set; } = 50; // -1 means unlimited
        
        public bool HasApiAccess { get; set; } = true;
        
        public bool HasPrioritySupport { get; set; } = false;
        
        public bool Has24x7Support { get; set; } = false;
        
        public bool HasCustomBranding { get; set; } = false;
        
        public bool HasAdvancedReporting { get; set; } = false;
        
        public bool HasCustomIntegrations { get; set; } = false;
        
        public bool HasKnowledgeBase { get; set; } = true;
        
        public bool HasEmailIntegration { get; set; } = true;
        
        public bool HasCannedResponses { get; set; } = false;
        
        public bool HasCSATSurveys { get; set; } = false;
        
        // Display settings
        public int DisplayOrder { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public bool IsRecommended { get; set; } = false;
        
        public string? Features { get; set; } // JSON array of feature descriptions
        
        // Navigation properties
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        
        // Helper methods
        public bool IsFreemium => Type == PlanType.Freemium;
        
        public bool IsUnlimitedTickets => MonthlyTicketLimit == -1;
        
        public string GetDisplayPrice()
        {
            if (Price == 0)
                return "رایگان";
            
            return $"{Price:N0} {Currency}";
        }
        
        public List<string> GetFeatureList()
        {
            var features = new List<string>();
            
            if (IsUnlimitedTickets)
                features.Add("تیکت نامحدود");
            else
                features.Add($"{MonthlyTicketLimit:N0} تیکت در ماه");
            
            if (HasEmailIntegration)
                features.Add("یکپارچه‌سازی ایمیل");
            
            if (HasKnowledgeBase)
                features.Add("پایگاه دانش");
            
            if (HasApiAccess)
                features.Add("دسترسی API");
            
            if (HasCannedResponses)
                features.Add("پاسخ‌های آماده");
            
            if (HasCSATSurveys)
                features.Add("نظرسنجی رضایت");
            
            if (HasAdvancedReporting)
                features.Add("گزارش‌های پیشرفته");
            
            if (HasCustomBranding)
                features.Add("برندینگ اختصاصی");
            
            if (HasPrioritySupport)
                features.Add("پشتیبانی اولویت‌دار");
            
            if (Has24x7Support)
                features.Add("پشتیبانی ۲۴/۷");
            
            if (HasCustomIntegrations)
                features.Add("یکپارچه‌سازی سفارشی");
            
            return features;
        }
    }
}