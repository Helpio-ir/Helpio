using Helpio.Dashboard.Services;
using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Dashboard.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OrganizationsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public OrganizationsController(ICurrentUserContext userContext, ApplicationDbContext context)
            : base(userContext)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var organizations = await _context.Organizations
                .Include(o => o.Branches)
                .Include(o => o.ApiKeys)
                .OrderBy(o => o.Name)
                .ToListAsync();

            return View(organizations);
        }

        public async Task<IActionResult> Details(int id)
        {
            var organization = await _context.Organizations
                .Include(o => o.Branches)
                    .ThenInclude(b => b.Teams)
                        .ThenInclude(t => t.SupportAgents)
                            .ThenInclude(sa => sa.User)
                .Include(o => o.TicketCategories)
                .Include(o => o.ApiKeys)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (organization == null)
            {
                return NotFound();
            }

            // Get subscription information
            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .Where(s => s.OrganizationId == id && s.IsActive)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();

            ViewBag.Subscription = subscription;

            return View(organization);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Organization());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Organization organization)
        {
            if (ModelState.IsValid)
            {
                organization.CreatedAt = DateTime.UtcNow;
                _context.Organizations.Add(organization);
                await _context.SaveChangesAsync();

                TempData["Success"] = "سازمان با موفقیت ایجاد شد.";
                return RedirectToAction(nameof(Details), new { id = organization.Id });
            }

            return View(organization);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null)
            {
                return NotFound();
            }

            return View(organization);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Organization organization)
        {
            if (id != organization.Id)
            {
                return BadRequest();
            }

            var existingOrg = await _context.Organizations.FindAsync(id);
            if (existingOrg == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                existingOrg.Name = organization.Name;
                existingOrg.Description = organization.Description;
                existingOrg.Email = organization.Email;
                existingOrg.PhoneNumber = organization.PhoneNumber;
                existingOrg.Address = organization.Address;
                existingOrg.IsActive = organization.IsActive;
                existingOrg.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                TempData["Success"] = "اطلاعات سازمان با موفقیت به‌روزرسانی شد.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(organization);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null)
            {
                return NotFound();
            }

            organization.IsActive = !organization.IsActive;
            organization.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"سازمان {(organization.IsActive ? "فعال" : "غیرفعال")} شد.";
            return RedirectToAction(nameof(Details), new { id });
        }

        /// <summary>
        /// Action for creating test subscription for an organization - For testing purposes only
        /// Remove this in production
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTestSubscription(int id)
        {
            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null)
            {
                return NotFound();
            }

            try
            {
                // Check if subscription already exists
                var existingSubscription = await _context.Subscriptions
                    .Where(s => s.OrganizationId == id && s.IsActive)
                    .FirstOrDefaultAsync();

                if (existingSubscription != null)
                {
                    TempData["Warning"] = $"سازمان {organization.Name} قبلاً دارای اشتراک فعال است.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // First, create or get the Professional plan
                var professionalPlan = await _context.Plans
                    .FirstOrDefaultAsync(p => p.Type == Helpio.Ir.Domain.Entities.Business.PlanType.Professional);

                if (professionalPlan == null)
                {
                    // Create a basic Professional plan if it doesn't exist
                    professionalPlan = new Helpio.Ir.Domain.Entities.Business.Plan
                    {
                        Name = "Professional Plan",
                        Description = "طرح حرفه‌ای با 1000 تیکت در ماه",
                        Type = Helpio.Ir.Domain.Entities.Business.PlanType.Professional,
                        Price = 1500000,
                        Currency = "IRR",
                        BillingCycleDays = 30,
                        MonthlyTicketLimit = 1000,
                        HasApiAccess = true,
                        HasPrioritySupport = true,
                        HasAdvancedReporting = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Plans.Add(professionalPlan);
                    await _context.SaveChangesAsync();
                }

                // Create Professional subscription for testing
                var testSubscription = new Helpio.Ir.Domain.Entities.Business.Subscription
                {
                    Name = "Professional Plan - Test",
                    Description = "طرح حرفه‌ای با 1000 تیکت در ماه برای تست",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(1), // 1 year subscription
                    Status = Helpio.Ir.Domain.Entities.Business.SubscriptionStatus.Active,
                    PlanId = professionalPlan.Id,
                    OrganizationId = id,
                    IsActive = true,
                    CurrentPeriodTicketCount = Random.Shared.Next(5, 50), // Random usage for testing
                    CurrentPeriodStartDate = DateTime.UtcNow.Date.AddDays(1 - DateTime.UtcNow.Day),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Subscriptions.Add(testSubscription);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"اشتراک Professional برای سازمان {organization.Name} با موفقیت ایجاد شد. ({testSubscription.CurrentPeriodTicketCount}/1000 تیکت استفاده شده)";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"خطا در ایجاد اشتراک تست: {ex.Message}";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}