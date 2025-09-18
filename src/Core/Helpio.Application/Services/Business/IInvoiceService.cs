using Helpio.Ir.Domain.Entities.Business;

namespace Helpio.Ir.Application.Services.Business
{
    public interface IInvoiceService
    {
        Task<Invoice> CreateInvoiceAsync(int subscriptionId, int planId, DateTime billingPeriodStart, DateTime billingPeriodEnd);
        Task<Invoice?> GetInvoiceByIdAsync(int invoiceId);
        Task<IEnumerable<Invoice>> GetInvoicesByOrganizationAsync(int organizationId, int page = 1, int pageSize = 10);
        Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync();
        Task<bool> MarkInvoiceAsPaidAsync(int invoiceId, string paymentMethod, string paymentReference);
        Task<bool> CancelInvoiceAsync(int invoiceId, string reason);
        Task<byte[]> GenerateInvoicePdfAsync(int invoiceId);
        Task<string> GetNextInvoiceNumberAsync();
        Task SendInvoiceByEmailAsync(int invoiceId);
    }
}