using Microsoft.AspNetCore.Mvc;
using Helpio.web.Models;
using Helpio.Ir.Application.Services.Business;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Helpio.web.Controllers
{
    public class SubscriptionController : Controller
    {
        private readonly ILogger<SubscriptionController> _logger;
        private readonly IPlanService _planService;
        private readonly ApplicationDbContext _context;

        public SubscriptionController(
            ILogger<SubscriptionController> logger,
            IPlanService planService,
            ApplicationDbContext context)
        {
            _logger = logger;
            _planService = planService;
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateFreemiumSubscription()
        {
            // For the web project, this would redirect to registration/login
            // since users need to be authenticated to create subscriptions
            TempData["Message"] = "برای ایجاد اشتراک رایگان، ابتدا نیاز به ثبت‌نام دارید.";
            return RedirectToAction("Register", "Account");
        }

        [HttpGet]
        public async Task<IActionResult> ContactSales(int? planId)
        {
            var model = new ContactSalesViewModel
            {
                PlanId = planId
            };

            if (planId.HasValue)
            {
                try
                {
                    var plan = await _context.Plans.FindAsync(planId.Value);
                    if (plan != null && plan.IsActive)
                    {
                        ViewBag.SelectedPlan = plan;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading plan {PlanId}", planId);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ContactSales(ContactSalesViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // TODO: در اینجا می‌توانید ایمیل را به تیم فروش ارسال کنید
                // یا درخواست را در دیتابیس ذخیره کنید

                _logger.LogInformation("Sales contact request from {Email} for organization {Organization}", 
                    model.ContactEmail, model.OrganizationName);

                TempData["SuccessMessage"] = "درخواست شما با موفقیت ارسال شد. تیم فروش ما در اسرع وقت با شما تماس خواهد گرفت.";
                return RedirectToAction("Pricing", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing sales contact request");
                TempData["ErrorMessage"] = "خطا در ارسال درخواست. لطفاً مجدداً تلاش کنید.";
                return View(model);
            }
        }
    }
}