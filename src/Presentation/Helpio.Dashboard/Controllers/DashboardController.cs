using Helpio.Dashboard.Services;
using Helpio.Dashboard.Models;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Helpio.Dashboard.Controllers
{
    public class DashboardController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ICurrentUserContext userContext, ApplicationDbContext context)
            : base(userContext)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardStatsViewModel();

            try
            {
                // Get subscription information for the organization
                if (CurrentOrganizationId.HasValue)
                {
                    var subscriptionLimitService = HttpContext.RequestServices.GetRequiredService<Helpio.Ir.Application.Services.Business.ISubscriptionLimitService>();
                    ViewBag.SubscriptionInfo = await subscriptionLimitService.GetSubscriptionLimitInfoAsync(CurrentOrganizationId.Value);
                }

                // Get tickets statistics based on user role
                var ticketsQuery = _context.Tickets
                    .Include(t => t.Team)
                        .ThenInclude(t => t.Branch)
                    .AsQueryable();

                if (IsCurrentUserAdmin)
                {
                    // Admin sees all tickets
                    model.TotalTickets = await ticketsQuery.CountAsync();
                    model.OpenTickets = await ticketsQuery.CountAsync(t => t.TicketStateId == 1);
                    model.ClosedTickets = await ticketsQuery.CountAsync(t => t.TicketStateId == 3);
                }
                else if (IsCurrentUserManager && CurrentOrganizationId.HasValue)
                {
                    // Manager sees organization tickets
                    ticketsQuery = ticketsQuery.Where(t => t.Team.Branch.OrganizationId == CurrentOrganizationId.Value);
                    model.TotalTickets = await ticketsQuery.CountAsync();
                    model.OpenTickets = await ticketsQuery.CountAsync(t => t.TicketStateId == 1);
                    model.ClosedTickets = await ticketsQuery.CountAsync(t => t.TicketStateId == 3);
                }
                else if (IsCurrentUserAgent)
                {
                    // Agent sees only assigned tickets
                    ticketsQuery = ticketsQuery.Where(t => 
                        t.SupportAgentId == UserContext.CurrentSupportAgent!.Id ||
                        (t.TeamId == CurrentTeamId && t.SupportAgentId == null));
                    model.TotalTickets = await ticketsQuery.CountAsync();
                    model.OpenTickets = await ticketsQuery.CountAsync(t => t.TicketStateId == 1);
                    model.ClosedTickets = await ticketsQuery.CountAsync(t => t.TicketStateId == 3);
                    model.MyTickets = model.TotalTickets; // For agents, all accessible tickets are "my tickets"
                }

                // Get customers count
                if (IsCurrentUserAdmin)
                {
                    model.TotalCustomers = await _context.Customers.CountAsync();
                }
                else if (CurrentOrganizationId.HasValue)
                {
                    model.TotalCustomers = await _context.Customers
                        .CountAsync(c => c.OrganizationId == CurrentOrganizationId.Value);
                }

                // Get support agents count
                if (IsCurrentUserAdmin)
                {
                    model.TotalAgents = await _context.SupportAgents.CountAsync();
                }
                else if (CurrentOrganizationId.HasValue)
                {
                    model.TotalAgents = await _context.SupportAgents
                        .Include(sa => sa.Team)
                            .ThenInclude(t => t.Branch)
                        .CountAsync(sa => sa.Team.Branch.OrganizationId == CurrentOrganizationId.Value);
                }
            }
            catch (Exception ex)
            {
                // Log the exception in a real application
                TempData["Error"] = $"خطا در بارگذاری داشبورد: {ex.Message}";
            }

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}