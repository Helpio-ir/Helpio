using Helpio.Dashboard.Services;
using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Domain.Entities.Ticketing;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Dashboard.Controllers
{
    public class TicketsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public TicketsController(ICurrentUserContext userContext, ApplicationDbContext context)
            : base(userContext)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var tickets = await GetAccessibleTicketsAsync();
            return View(tickets);
        }

        public async Task<IActionResult> Details(int id)
        {
            var ticket = await GetTicketByIdAsync(id);
            if (ticket == null || !CanAccessTicket(ticket))
            {
                return NotFound();
            }

            return View(ticket);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            ViewBag.Categories = await GetAccessibleCategoriesAsync();
            ViewBag.Customers = await GetAccessibleCustomersAsync();
            return View(new Ticket());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ticket ticket)
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                // Set default values
                ticket.CreatedAt = DateTime.UtcNow;
                ticket.TicketStateId = 1; // Default to "Open"

                if (!IsCurrentUserAdmin && CurrentTeamId.HasValue)
                {
                    ticket.TeamId = CurrentTeamId.Value;
                }

                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();

                TempData["Success"] = "تیکت با موفقیت ایجاد شد.";
                return RedirectToAction(nameof(Details), new { id = ticket.Id });
            }

            ViewBag.Categories = await GetAccessibleCategoriesAsync();
            ViewBag.Customers = await GetAccessibleCustomersAsync();
            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignToMe(int id)
        {
            var ticket = await GetTicketByIdAsync(id);
            if (ticket == null || !CanAccessTicket(ticket))
            {
                return NotFound();
            }

            if (UserContext.CurrentSupportAgent == null)
            {
                return BadRequest("شما کارشناس پشتیبانی نیستید.");
            }

            ticket.SupportAgentId = UserContext.CurrentSupportAgent.Id;
            ticket.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = "تیکت به شما اختصاص داده شد.";
            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task<List<Ticket>> GetAccessibleTicketsAsync()
        {
            var query = _context.Tickets
                .Include(t => t.Customer)
                .Include(t => t.TicketState)
                .Include(t => t.TicketCategory)
                .Include(t => t.SupportAgent)
                    .ThenInclude(sa => sa.User)
                .Include(t => t.Team)
                .AsQueryable();

            if (IsCurrentUserAdmin)
            {
                // Admin sees all tickets
                return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
            }
            else if (IsCurrentUserManager && CurrentOrganizationId.HasValue)
            {
                // Manager sees organization tickets
                query = query.Where(t => t.Team.Branch.OrganizationId == CurrentOrganizationId.Value);
            }
            else if (IsCurrentUserAgent)
            {
                // Agent sees only assigned tickets or team tickets
                query = query.Where(t =>
                    t.SupportAgentId == UserContext.CurrentSupportAgent!.Id ||
                    (t.TeamId == CurrentTeamId && t.SupportAgentId == null));
            }
            else
            {
                return new List<Ticket>();
            }

            return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        }

        private async Task<Ticket?> GetTicketByIdAsync(int id)
        {
            return await _context.Tickets
                .Include(t => t.Customer)
                .Include(t => t.TicketState)
                .Include(t => t.TicketCategory)
                .Include(t => t.SupportAgent)
                    .ThenInclude(sa => sa.User)
                .Include(t => t.Team)
                .Include(t => t.Responses)
                    .ThenInclude(r => r.User)
                .Include(t => t.Notes)
                    .ThenInclude(n => n.SupportAgent)
                        .ThenInclude(sa => sa.User)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        private bool CanAccessTicket(Ticket ticket)
        {
            if (IsCurrentUserAdmin) return true;

            if (IsCurrentUserManager && CurrentOrganizationId.HasValue)
            {
                return ticket.Team?.Branch?.OrganizationId == CurrentOrganizationId.Value;
            }

            if (IsCurrentUserAgent)
            {
                return ticket.SupportAgentId == UserContext.CurrentSupportAgent?.Id ||
                       (ticket.TeamId == CurrentTeamId && ticket.SupportAgentId == null);
            }

            return false;
        }

        private async Task<List<TicketCategory>> GetAccessibleCategoriesAsync()
        {
            var query = _context.TicketCategories.AsQueryable();

            if (!IsCurrentUserAdmin && CurrentOrganizationId.HasValue)
            {
                query = query.Where(tc => tc.OrganizationId == CurrentOrganizationId.Value);
            }

            return await query.ToListAsync();
        }

        private async Task<List<Customer>> GetAccessibleCustomersAsync()
        {
            // For now, return all customers. In a real scenario, this would be filtered based on organization
            return await _context.Customers.Take(100).ToListAsync();
        }
    }
}