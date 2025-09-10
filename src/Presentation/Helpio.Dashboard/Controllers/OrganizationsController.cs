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
    }
}