using Helpio.Dashboard.Services;
using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Dashboard.Controllers
{
    public class CustomersController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public CustomersController(ICurrentUserContext userContext, ApplicationDbContext context)
            : base(userContext)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await GetAccessibleCustomersAsync();
            return View(customers);
        }

        public async Task<IActionResult> Details(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Tickets)
                    .ThenInclude(t => t.TicketState)
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null || !CanAccessCustomer(customer))
            {
                return NotFound();
            }

            return View(customer);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            return View(new Customer());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                customer.CreatedAt = DateTime.UtcNow;
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                TempData["Success"] = "مشتری با موفقیت ایجاد شد.";
                return RedirectToAction(nameof(Details), new { id = customer.Id });
            }

            return View(customer);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null || !CanAccessCustomer(customer))
            {
                return NotFound();
            }

            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            if (id != customer.Id)
            {
                return BadRequest();
            }

            var existingCustomer = await _context.Customers.FindAsync(id);
            if (existingCustomer == null || !CanAccessCustomer(existingCustomer))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                existingCustomer.FirstName = customer.FirstName;
                existingCustomer.LastName = customer.LastName;
                existingCustomer.Email = customer.Email;
                existingCustomer.PhoneNumber = customer.PhoneNumber;
                existingCustomer.Address = customer.Address;
                existingCustomer.CompanyName = customer.CompanyName;
                existingCustomer.IsDeleted = customer.IsDeleted;
                existingCustomer.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                TempData["Success"] = "اطلاعات مشتری با موفقیت به‌روزرسانی شد.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(customer);
        }

        private async Task<List<Customer>> GetAccessibleCustomersAsync()
        {
            var query = _context.Customers
                .Include(c => c.Tickets)
                    .ThenInclude(t => t.TicketState)
                .Where(c => !c.IsDeleted) // Filter out deleted customers
                .AsQueryable();

            if (IsCurrentUserAdmin)
            {
                // Admin sees all customers
                return await query.OrderBy(c => c.LastName).ToListAsync();
            }
            else if (IsCurrentUserManager && CurrentOrganizationId.HasValue)
            {
                // Manager sees customers who have tickets with their organization
                query = query.Where(c => c.Tickets.Any(t => t.Team.Branch.OrganizationId == CurrentOrganizationId.Value));
            }
            else if (IsCurrentUserAgent && CurrentTeamId.HasValue)
            {
                // Agent sees customers who have tickets assigned to them or their team
                query = query.Where(c => c.Tickets.Any(t =>
                    t.SupportAgentId == UserContext.CurrentSupportAgent!.Id ||
                    t.TeamId == CurrentTeamId.Value));
            }
            else
            {
                return new List<Customer>();
            }

            return await query.OrderBy(c => c.LastName).ToListAsync();
        }

        private bool CanAccessCustomer(Customer customer)
        {
            if (IsCurrentUserAdmin) return true;

            // For now, use simple logic. In real scenario, check if customer has tickets in user's organization/team
            return true;
        }
    }
}