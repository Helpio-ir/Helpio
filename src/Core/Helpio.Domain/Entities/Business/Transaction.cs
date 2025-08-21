using System;

namespace Helpio.Ir.Domain.Entities.Business
{
    public enum TransactionType
    {
        Payment = 1,
        Refund = 2,
        Charge = 3
    }

    public enum TransactionStatus
    {
        Pending = 1,
        Completed = 2,
        Failed = 3,
        Cancelled = 4
    }

    public class Transaction : BaseEntity
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public TransactionType Type { get; set; }
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
        public string? Reference { get; set; }
        public string? Description { get; set; }
        public DateTime ProcessedAt { get; set; }
        
        // Navigation properties - Payment entity would be created separately if needed
    }
}