using Helpio.Dashboard.Models;
using Helpio.Dashboard.Services;
using Helpio.Ir.Application.Services.Business;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Dashboard.Controllers
{
    public class SubscriptionController : BaseController
    {
        private readonly ISubscriptionLimitService _subscriptionLimitService;
        private readonly ApplicationDbContext _context;

        public SubscriptionController(
            ICurrentUserContext userContext,
            ISubscriptionLimitService subscriptionLimitService,
            ApplicationDbContext context)
            : base(userContext)
        {
            _subscriptionLimitService = subscriptionLimitService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (!CurrentOrganizationId.HasValue)
            {
                TempData["Error"] = "سازمان شما شناسایی نشد. لطفاً با مدیر سیستم تماس بگیرید.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                // Get current subscription info
                var limitInfo = await _subscriptionLimitService.GetSubscriptionLimitInfoAsync(CurrentOrganizationId.Value);
                var activeSubscription = await _subscriptionLimitService.GetActiveSubscriptionAsync(CurrentOrganizationId.Value);

                ViewBag.LimitInfo = limitInfo;
                ViewBag.ActiveSubscription = activeSubscription;

                // Get subscription history
                var subscriptions = await _context.Subscriptions
                    .Include(s => s.Plan)
                    .Where(s => s.OrganizationId == CurrentOrganizationId.Value)
                    .OrderByDescending(s => s.CreatedAt)
                    .ToListAsync();

                return View(subscriptions);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "خطا در بارگذاری اطلاعات اشتراک: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Status()
        {
            if (!CurrentOrganizationId.HasValue)
            {
                TempData["Error"] = "اطلاعات سازمان شما یافت نشد.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var limitInfo = await _subscriptionLimitService.GetSubscriptionLimitInfoAsync(CurrentOrganizationId.Value);

                return View(limitInfo);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "خطا در دریافت اطلاعات اشتراک. لطفاً مجدداً تلاش کنید.";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Usage()
        {
            if (!CurrentOrganizationId.HasValue)
            {
                TempData["Error"] = "سازمان شما شناسایی نشد.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var limitInfo = await _subscriptionLimitService.GetSubscriptionLimitInfoAsync(CurrentOrganizationId.Value);

                // Get monthly ticket creation statistics
                var currentMonth = DateTime.UtcNow.Date.AddDays(1 - DateTime.UtcNow.Day);
                var tickets = await _context.Tickets
                    .Include(t => t.Team)
                        .ThenInclude(t => t.Branch)
                    .Where(t => t.Team.Branch.OrganizationId == CurrentOrganizationId.Value
                               && t.CreatedAt >= currentMonth)
                    .GroupBy(t => t.CreatedAt.Date)
                    .Select(g => new { Date = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Date)
                    .ToListAsync();

                ViewBag.LimitInfo = limitInfo;
                ViewBag.DailyTickets = tickets;

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "خطا در بارگذاری آمار استفاده: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Upgrade()
        {
            if (!CurrentOrganizationId.HasValue)
            {
                TempData["Error"] = "سازمان شما شناسایی نشد.";
                return RedirectToAction("Index", "Home");
            }

            var limitInfo = await _subscriptionLimitService.GetSubscriptionLimitInfoAsync(CurrentOrganizationId.Value);
            ViewBag.LimitInfo = limitInfo;

            // Sample subscription plans
            ViewBag.Plans = GetAvailablePlans();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFreemiumSubscription()
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            if (!CurrentOrganizationId.HasValue)
            {
                TempData["Error"] = "سازمان شما شناسایی نشد.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                // Check if organization already has an active subscription
                var existingSubscription = await _subscriptionLimitService.GetActiveSubscriptionAsync(CurrentOrganizationId.Value);
                if (existingSubscription != null)
                {
                    TempData["Warning"] = "سازمان شما قبلاً دارای اشتراک فعال است.";
                    return RedirectToAction("Index");
                }

                // First, create or get the Freemium plan
                var freemiumPlan = await _context.Plans
                    .FirstOrDefaultAsync(p => p.Type == Helpio.Ir.Domain.Entities.Business.PlanType.Freemium);

                if (freemiumPlan == null)
                {
                    // Create a basic Freemium plan if it doesn't exist
                    freemiumPlan = new Helpio.Ir.Domain.Entities.Business.Plan
                    {
                        Name = "Freemium Plan",
                        Description = "طرح رایگان با محدودیت ۵۰ تیکت در ماه",
                        Type = Helpio.Ir.Domain.Entities.Business.PlanType.Freemium,
                        Price = 0,
                        Currency = "IRR",
                        BillingCycleDays = 30,
                        MonthlyTicketLimit = 50,
                        HasApiAccess = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Plans.Add(freemiumPlan);
                    await _context.SaveChangesAsync();
                }

                // Create freemium subscription
                var subscription = new Helpio.Ir.Domain.Entities.Business.Subscription
                {
                    Name = "Freemium Plan",
                    Description = "طرح رایگان با محدودیت ۵۰ تیکت در ماه",
                    StartDate = DateTime.UtcNow,
                    EndDate = null, // Freemium doesn't expire
                    Status = Helpio.Ir.Domain.Entities.Business.SubscriptionStatus.Active,
                    PlanId = freemiumPlan.Id,
                    OrganizationId = CurrentOrganizationId.Value,
                    IsActive = true,
                    CurrentPeriodTicketCount = 0,
                    CurrentPeriodStartDate = DateTime.UtcNow.Date.AddDays(1 - DateTime.UtcNow.Day),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Subscriptions.Add(subscription);
                await _context.SaveChangesAsync();

                TempData["Success"] = "اشتراک فریمیوم با موفقیت فعال شد. شما می‌توانید تا ۵۰ تیکت در ماه ایجاد کنید.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "خطا در ایجاد اشتراک: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ContactSales(int? planId)
        {
            if (planId.HasValue)
            {
                var plan = await _context.Plans.FindAsync(planId.Value);
                if (plan != null && plan.IsActive)
                {
                    ViewBag.SelectedPlan = plan;
                }
            }

            var model = new ContactSalesViewModel
            {
                PlanId = planId,
                OrganizationName = UserContext.CurrentOrganization?.Name ?? "",
                ContactName = UserContext.UserFullName ?? "",
                ContactEmail = UserContext.UserEmail ?? "",
                ContactPhone = "" // Phone number not available in current user context
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContactSales(ContactSalesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                if (model.PlanId.HasValue)
                {
                    var plan = await _context.Plans.FindAsync(model.PlanId.Value);
                    if (plan != null && plan.IsActive)
                    {
                        ViewBag.SelectedPlan = plan;
                    }
                }
                return View(model);
            }

            try
            {
                // در اینجا می‌توانید ایمیل را به تیم فروش ارسال کنید
                // یا درخواست را در دیتابیس ذخیره کنید

                // برای الان فقط پیام موفقیت نمایش می‌دهیم
                TempData["Success"] = "درخواست شما با موفقیت ارسال شد. تیم فروش ما در اسرع وقت با شما تماس خواهد گرفت.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "خطا در ارسال درخواست: " + ex.Message;
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLimitInfo()
        {
            if (!CurrentOrganizationId.HasValue)
            {
                return Json(new { success = false, message = "اطلاعات سازمان یافت نشد." });
            }

            try
            {
                var limitInfo = await _subscriptionLimitService.GetSubscriptionLimitInfoAsync(CurrentOrganizationId.Value);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        canCreateTickets = limitInfo.CanCreateTickets,
                        remainingTickets = limitInfo.RemainingTickets,
                        monthlyLimit = limitInfo.MonthlyLimit,
                        currentMonthUsage = limitInfo.CurrentMonthUsage,
                        planType = limitInfo.PlanType.ToString(),
                        isFreemium = limitInfo.IsFreemium,
                        currentMonthStartDate = limitInfo.CurrentMonthStartDate.ToString("yyyy-MM-dd"),
                        limitationMessage = limitInfo.LimitationMessage,
                        percentageUsed = limitInfo.MonthlyLimit > 0 ?
                            Math.Round((double)limitInfo.CurrentMonthUsage / limitInfo.MonthlyLimit * 100, 1) : 0
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطا در دریافت اطلاعات محدودیت‌ها." });
            }
        }

        private List<object> GetAvailablePlans()
        {
            return new List<object>
            {
                new
                {
                    Name = "Basic",
                    Description = "برای تیم‌های کوچک",
                    Price = 500000,
                    Currency = "تومان",
                    TicketLimit = 200,
                    Features = new[] { "۲۰۰ تیکت در ماه", "پشتیبانی ایمیل", "گزارش‌گیری پایه" }
                },
                new
                {
                    Name = "Professional",
                    Description = "برای تیم‌های متوسط",
                    Price = 1500000,
                    Currency = "تومان",
                    TicketLimit = 1000,
                    Features = new[] { "۱۰۰۰ تیکت در ماه", "پشتیبانی اولویت‌دار", "گزارش‌گیری پیشرفته", "API کامل" }
                },
                new
                {
                    Name = "Enterprise",
                    Description = "برای سازمان‌های بزرگ",
                    Price = 5000000,
                    Currency = "تومان",
                    TicketLimit = -1, // Unlimited
                    Features = new[] { "تیکت نامحدود", "پشتیبانی ۲۴/۷", "گزارش‌گیری تخصصی", "یکپارچه‌سازی سفارشی" }
                }
            };
        }
    }
}
