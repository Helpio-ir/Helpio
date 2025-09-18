namespace Helpio.Ir.Application.Services.Business
{
    public interface INotificationService
    {
        Task SendSubscriptionLimitWarningAsync(int organizationId, int remainingTickets, int totalLimit);
        Task SendSubscriptionExpiredAsync(int organizationId, DateTime expiredDate);
        Task SendSubscriptionUpgradedAsync(int organizationId, string newPlanName);
        Task SendInvoiceCreatedAsync(int invoiceId);
        Task SendInvoiceOverdueAsync(int invoiceId, int daysOverdue);
        Task SendPaymentSuccessfulAsync(int organizationId, decimal amount, string planName);
        Task SendPaymentFailedAsync(int organizationId, decimal amount, string reason);
        Task SendWelcomeMessageAsync(int organizationId, string planName);
    }
}