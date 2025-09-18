using System.ComponentModel.DataAnnotations;

namespace Helpio.Ir.Domain.Entities.Business
{
    public enum InvoiceStatus
    {
        Draft = 1,
        Sent = 2,
        Paid = 3,
        Overdue = 4,
        Cancelled = 5,
        Refunded = 6
    }

    public class Invoice : BaseEntity
    {
        [Required]
        [MaxLength(20)]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required]
        public int OrganizationId { get; set; }

        [Required]
        public int SubscriptionId { get; set; }

        public int? PlanId { get; set; }

        public DateTime IssueDate { get; set; } = DateTime.UtcNow;

        public DateTime DueDate { get; set; }

        public DateTime? PaidDate { get; set; }

        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

        public decimal SubTotal { get; set; }

        public decimal TaxRate { get; set; } = 0.09m; // 9% VAT

        public decimal TaxAmount { get; set; }

        public decimal DiscountAmount { get; set; } = 0;

        public decimal TotalAmount { get; set; }

        [MaxLength(3)]
        public string Currency { get; set; } = "IRR";

        public string? Notes { get; set; }

        public string? PaymentMethod { get; set; }

        public string? PaymentReference { get; set; }

        // Billing period
        public DateTime BillingPeriodStart { get; set; }

        public DateTime BillingPeriodEnd { get; set; }

        // Navigation properties
        public virtual Core.Organization Organization { get; set; } = null!;

        public virtual Subscription Subscription { get; set; } = null!;

        public virtual Plan? Plan { get; set; }

        public virtual ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();

        // Helper methods
        public bool IsOverdue => Status != InvoiceStatus.Paid && Status != InvoiceStatus.Cancelled && DueDate < DateTime.UtcNow;

        public int DaysOverdue => IsOverdue ? (DateTime.UtcNow - DueDate).Days : 0;

        public void CalculateTotals()
        {
            SubTotal = Items.Sum(i => i.Total);
            TaxAmount = SubTotal * TaxRate;
            TotalAmount = SubTotal + TaxAmount - DiscountAmount;
        }

        public string GetDisplayTotal()
        {
            return $"{TotalAmount:N0} {Currency}";
        }

        public void MarkAsPaid(string paymentMethod, string paymentReference)
        {
            Status = InvoiceStatus.Paid;
            PaidDate = DateTime.UtcNow;
            PaymentMethod = paymentMethod;
            PaymentReference = paymentReference;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public class InvoiceItem : BaseEntity
    {
        [Required]
        public int InvoiceId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;

        public int Quantity { get; set; } = 1;

        public decimal UnitPrice { get; set; }

        public decimal Total => Quantity * UnitPrice;

        public int DisplayOrder { get; set; }

        // Navigation properties
        public virtual Invoice Invoice { get; set; } = null!;
    }
}