using Helpio.Ir.Application.Common.Interfaces;
using Helpio.Ir.Domain.Entities.Business;
using Helpio.Ir.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Helpio.Ir.Application.Services.Business
{
    public class NotificationService : INotificationService
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<NotificationService> _logger;
        // TODO: اضافه کردن سرویس ایمیل و SMS

        public NotificationService(
            IApplicationDbContext context,
            ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SendSubscriptionLimitWarningAsync(int organizationId, int remainingTickets, int totalLimit)
        {
            try
            {
                var organization = await _context.Organizations.FindAsync(organizationId);
                if (organization == null) return;

                var message = remainingTickets <= 0 
                    ? $"سازمان {organization.Name}: به حد مجاز {totalLimit} تیکت در ماه رسیده‌اید. برای ایجاد تیکت بیشتر، لطفاً اشتراک خود را ارتقا دهید."
                    : $"سازمان {organization.Name}: تنها {remainingTickets} تیکت از {totalLimit} تیکت ماهانه باقی‌مانده است.";

                _logger.LogWarning("Subscription limit warning for organization {OrganizationId}: {Message}", 
                    organizationId, message);

                // TODO: ارسال ایمیل یا نوتیفیکیشن
                // await _emailService.SendAsync(organization.Email, "هشدار محدودیت اشتراک", message);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending subscription limit warning for organization {OrganizationId}", organizationId);
            }
        }

        public async Task SendSubscriptionExpiredAsync(int organizationId, DateTime expiredDate)
        {
            try
            {
                var organization = await _context.Organizations.FindAsync(organizationId);
                if (organization == null) return;

                var message = $"سازمان {organization.Name}: اشتراک شما در تاریخ {expiredDate:yyyy/MM/dd} منقضی شده است. برای ادامه استفاده از خدمات، لطفاً اشتراک خود را تمدید کنید.";

                _logger.LogWarning("Subscription expired for organization {OrganizationId} on {ExpiredDate}", 
                    organizationId, expiredDate);

                // TODO: ارسال ایمیل
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending subscription expired notification for organization {OrganizationId}", organizationId);
            }
        }

        public async Task SendSubscriptionUpgradedAsync(int organizationId, string newPlanName)
        {
            try
            {
                var organization = await _context.Organizations.FindAsync(organizationId);
                if (organization == null) return;

                var message = $"تبریک! اشتراک سازمان {organization.Name} با موفقیت به پلن {newPlanName} ارتقا یافت. اکنون می‌توانید از امکانات جدید استفاده کنید.";

                _logger.LogInformation("Subscription upgraded for organization {OrganizationId} to plan {PlanName}", 
                    organizationId, newPlanName);

                // TODO: ارسال ایمیل
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending subscription upgrade notification for organization {OrganizationId}", organizationId);
            }
        }

        public async Task SendInvoiceCreatedAsync(int invoiceId)
        {
            try
            {
                var invoice = await _context.Invoices
                    .Include(i => i.Organization)
                    .FirstOrDefaultAsync(i => i.Id == invoiceId);

                if (invoice == null) return;

                var message = $"فاکتور جدید شماره {invoice.InvoiceNumber} به مبلغ {invoice.GetDisplayTotal()} برای سازمان {invoice.Organization.Name} صادر شد. مهلت پرداخت: {invoice.DueDate:yyyy/MM/dd}";

                _logger.LogInformation("Invoice {InvoiceNumber} created for organization {OrganizationId}", 
                    invoice.InvoiceNumber, invoice.OrganizationId);

                // TODO: ارسال ایمیل
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending invoice created notification for invoice {InvoiceId}", invoiceId);
            }
        }

        public async Task SendInvoiceOverdueAsync(int invoiceId, int daysOverdue)
        {
            try
            {
                var invoice = await _context.Invoices
                    .Include(i => i.Organization)
                    .FirstOrDefaultAsync(i => i.Id == invoiceId);

                if (invoice == null) return;

                var message = $"فاکتور شماره {invoice.InvoiceNumber} به مبلغ {invoice.GetDisplayTotal()} {daysOverdue} روز از موعد مقرر گذشته است. لطفاً در اسرع وقت نسبت به پرداخت اقدام کنید.";

                _logger.LogWarning("Invoice {InvoiceNumber} is overdue by {DaysOverdue} days for organization {OrganizationId}", 
                    invoice.InvoiceNumber, daysOverdue, invoice.OrganizationId);

                // TODO: ارسال ایمیل
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending invoice overdue notification for invoice {InvoiceId}", invoiceId);
            }
        }

        public async Task SendPaymentSuccessfulAsync(int organizationId, decimal amount, string planName)
        {
            try
            {
                var organization = await _context.Organizations.FindAsync(organizationId);
                if (organization == null) return;

                var message = $"پرداخت شما به مبلغ {amount:N0} تومان برای پلن {planName} با موفقیت انجام شد. از اعتماد شما متشکریم.";

                _logger.LogInformation("Payment successful for organization {OrganizationId}, amount {Amount}, plan {PlanName}", 
                    organizationId, amount, planName);

                // TODO: ارسال ایمیل
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment successful notification for organization {OrganizationId}", organizationId);
            }
        }

        public async Task SendPaymentFailedAsync(int organizationId, decimal amount, string reason)
        {
            try
            {
                var organization = await _context.Organizations.FindAsync(organizationId);
                if (organization == null) return;

                var message = $"پرداخت شما به مبلغ {amount:N0} تومان ناموفق بود. دلیل: {reason}. لطفاً مجدداً تلاش کنید یا با پشتیبانی تماس بگیرید.";

                _logger.LogWarning("Payment failed for organization {OrganizationId}, amount {Amount}, reason {Reason}", 
                    organizationId, amount, reason);

                // TODO: ارسال ایمیل
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment failed notification for organization {OrganizationId}", organizationId);
            }
        }

        public async Task SendWelcomeMessageAsync(int organizationId, string planName)
        {
            try
            {
                var organization = await _context.Organizations.FindAsync(organizationId);
                if (organization == null) return;

                var message = $"خوش آمدید! ثبت‌نام شما در پلن {planName} با موفقیت انجام شد. اکنون می‌توانید از تمام امکانات سیستم تیکتینگ استفاده کنید.";

                _logger.LogInformation("Welcome message sent for organization {OrganizationId} with plan {PlanName}", 
                    organizationId, planName);

                // TODO: ارسال ایمیل خوش‌آمدگویی
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending welcome message for organization {OrganizationId}", organizationId);
            }
        }
    }
}