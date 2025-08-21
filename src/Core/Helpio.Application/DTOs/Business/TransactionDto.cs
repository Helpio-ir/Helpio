using Helpio.Ir.Application.DTOs.Common;

namespace Helpio.Ir.Application.DTOs.Business
{
    public enum TransactionTypeDto
    {
        Payment = 1,
        Refund = 2,
        Charge = 3
    }

    public enum TransactionStatusDto
    {
        Pending = 1,
        Completed = 2,
        Failed = 3,
        Cancelled = 4
    }

    public class TransactionDto : BaseDto
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public TransactionTypeDto Type { get; set; }
        public TransactionStatusDto Status { get; set; }
        public string? Reference { get; set; }
        public string? Description { get; set; }
        public DateTime ProcessedAt { get; set; }
        
        // Computed Properties
        public bool IsCompleted => Status == TransactionStatusDto.Completed;
        public bool IsPending => Status == TransactionStatusDto.Pending;
        public bool HasFailed => Status == TransactionStatusDto.Failed;
        public TimeSpan ProcessingTime => ProcessedAt - CreatedAt;
        
        // For backward compatibility with interface
        public string? ReferenceNumber => Reference;
        public DateTime TransactionDate => CreatedAt;
        public DateTime? ProcessedDate => ProcessedAt;
        public string? ProcessorReference => Reference;
        public string? FailureReason => Status == TransactionStatusDto.Failed ? Description : null;
        public decimal? Fee => null; // Not supported in current model
        public decimal NetAmount => Amount; // No fees in current model
    }

    public class CreateTransactionDto
    {
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public TransactionTypeDto Type { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateTransactionDto
    {
        public TransactionStatusDto Status { get; set; }
        public string? Description { get; set; }
    }
}