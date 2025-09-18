using Helpio.Ir.Application.Common.Interfaces;
using Helpio.Ir.Domain.Entities.Business;
using Helpio.Ir.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Helpio.Ir.Application.Services.Business
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<InvoiceService> _logger;

        public InvoiceService(
            IApplicationDbContext context,
            ILogger<InvoiceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Invoice> CreateInvoiceAsync(int subscriptionId, int planId, DateTime billingPeriodStart, DateTime billingPeriodEnd)
        {
            try
            {
                var subscription = await _context.Subscriptions
                    .Include(s => s.Plan)
                    .Include(s => s.Organization)
                    .FirstOrDefaultAsync(s => s.Id == subscriptionId);

                if (subscription == null)
                    throw new ArgumentException("اشتراک یافت نشد", nameof(subscriptionId));

                var plan = await _context.Plans.FindAsync(planId);
                if (plan == null)
                    throw new ArgumentException("پلن یافت نشد", nameof(planId));

                var invoiceNumber = await GetNextInvoiceNumberAsync();

                var invoice = new Invoice
                {
                    InvoiceNumber = invoiceNumber,
                    OrganizationId = subscription.OrganizationId,
                    SubscriptionId = subscriptionId,
                    PlanId = planId,
                    IssueDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(30), // 30 روز مهلت پرداخت
                    Status = InvoiceStatus.Draft,
                    BillingPeriodStart = billingPeriodStart,
                    BillingPeriodEnd = billingPeriodEnd,
                    Currency = plan.Currency,
                    CreatedAt = DateTime.UtcNow
                };

                // اضافه کردن آیتم اشتراک
                var subscriptionItem = new InvoiceItem
                {
                    Description = $"اشتراک {plan.Name} - {billingPeriodStart:yyyy/MM/dd} تا {billingPeriodEnd:yyyy/MM/dd}",
                    Quantity = 1,
                    UnitPrice = subscription.GetPrice(),
                    DisplayOrder = 1,
                    CreatedAt = DateTime.UtcNow
                };

                invoice.Items.Add(subscriptionItem);
                invoice.CalculateTotals();

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created invoice {InvoiceNumber} for subscription {SubscriptionId}", 
                    invoiceNumber, subscriptionId);

                return invoice;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice for subscription {SubscriptionId}", subscriptionId);
                throw;
            }
        }

        public async Task<Invoice?> GetInvoiceByIdAsync(int invoiceId)
        {
            return await _context.Invoices
                .Include(i => i.Organization)
                .Include(i => i.Subscription)
                .Include(i => i.Plan)
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesByOrganizationAsync(int organizationId, int page = 1, int pageSize = 10)
        {
            return await _context.Invoices
                .Include(i => i.Plan)
                .Include(i => i.Items)
                .Where(i => i.OrganizationId == organizationId)
                .OrderByDescending(i => i.IssueDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.Invoices
                .Include(i => i.Organization)
                .Include(i => i.Subscription)
                .Where(i => i.Status != InvoiceStatus.Paid && 
                           i.Status != InvoiceStatus.Cancelled && 
                           i.DueDate < today)
                .ToListAsync();
        }

        public async Task<bool> MarkInvoiceAsPaidAsync(int invoiceId, string paymentMethod, string paymentReference)
        {
            try
            {
                var invoice = await _context.Invoices.FindAsync(invoiceId);
                if (invoice == null) return false;

                invoice.MarkAsPaid(paymentMethod, paymentReference);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Marked invoice {InvoiceId} as paid", invoiceId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking invoice {InvoiceId} as paid", invoiceId);
                return false;
            }
        }

        public async Task<bool> CancelInvoiceAsync(int invoiceId, string reason)
        {
            try
            {
                var invoice = await _context.Invoices.FindAsync(invoiceId);
                if (invoice == null) return false;

                invoice.Status = InvoiceStatus.Cancelled;
                invoice.Notes = $"لغو شده: {reason}";
                invoice.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Cancelled invoice {InvoiceId} with reason: {Reason}", invoiceId, reason);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling invoice {InvoiceId}", invoiceId);
                return false;
            }
        }

        public async Task<byte[]> GenerateInvoicePdfAsync(int invoiceId)
        {
            // TODO: پیاده‌سازی تولید PDF با استفاده از کتابخانه‌هایی مثل iTextSharp یا DinkToPdf
            await Task.Delay(100); // Placeholder
            return Array.Empty<byte>();
        }

        public async Task<string> GetNextInvoiceNumberAsync()
        {
            var lastInvoice = await _context.Invoices
                .OrderByDescending(i => i.CreatedAt)
                .FirstOrDefaultAsync();

            if (lastInvoice == null)
                return $"INV-{DateTime.UtcNow:yyyyMM}-0001";

            // استخراج شماره از آخرین فاکتور
            var currentMonth = DateTime.UtcNow.ToString("yyyyMM");
            var prefix = $"INV-{currentMonth}-";

            var lastNumber = 0;
            if (lastInvoice.InvoiceNumber.StartsWith(prefix))
            {
                var numberPart = lastInvoice.InvoiceNumber.Substring(prefix.Length);
                int.TryParse(numberPart, out lastNumber);
            }

            return $"{prefix}{(lastNumber + 1):D4}";
        }

        public async Task SendInvoiceByEmailAsync(int invoiceId)
        {
            // TODO: پیاده‌سازی ارسال ایمیل فاکتور
            _logger.LogInformation("Sending invoice {InvoiceId} by email", invoiceId);
            await Task.CompletedTask;
        }
    }
}