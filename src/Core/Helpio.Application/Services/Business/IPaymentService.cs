using Helpio.Ir.Domain.Entities.Business;

namespace Helpio.Ir.Application.Services.Business
{
    public interface IPaymentService
    {
        Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
        Task<PaymentResult> VerifyPaymentAsync(string paymentId, string verificationCode);
        Task<string> GeneratePaymentUrlAsync(int subscriptionId, int planId, decimal amount);
        Task<bool> RefundPaymentAsync(string paymentId, decimal amount, string reason);
    }

    public class PaymentRequest
    {
        public int OrganizationId { get; set; }
        public int PlanId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "IRR";
        public string Description { get; set; } = string.Empty;
        public string CallbackUrl { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserPhone { get; set; } = string.Empty;
    }

    public class PaymentResult
    {
        public bool IsSuccessful { get; set; }
        public string PaymentId { get; set; } = string.Empty;
        public string PaymentUrl { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string TransactionId { get; set; } = string.Empty;
    }

    public enum PaymentStatus
    {
        Pending = 1,
        Successful = 2,
        Failed = 3,
        Refunded = 4,
        Cancelled = 5
    }
}