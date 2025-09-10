using Helpio.Dashboard.Services;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Dashboard.Controllers;

public class DashboardController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        ICurrentUserContext userContext,
        ApplicationDbContext context,
        ILogger<DashboardController> logger)
        : base(userContext)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var stats = await GetDashboardStatsAsync();
        return View(stats);
    }

    private async Task<DashboardStatsViewModel> GetDashboardStatsAsync()
    {
        var orgId = CurrentOrganizationId;
        var teamId = CurrentTeamId;

        var stats = new DashboardStatsViewModel();

        if (IsCurrentUserAdmin)
        {
            // Admin sees all stats
            stats.TotalTickets = await _context.Tickets.CountAsync();
            stats.OpenTickets = await _context.Tickets.CountAsync(t => t.TicketState.Name == "Open");
            stats.ClosedTickets = await _context.Tickets.CountAsync(t => t.TicketState.Name == "Closed");
            stats.TotalCustomers = await _context.Customers.CountAsync();
            stats.TotalOrganizations = await _context.Organizations.CountAsync();
            stats.TotalAgents = await _context.SupportAgents.CountAsync();
        }
        else if (IsCurrentUserManager && orgId.HasValue)
        {
            // Manager sees organization stats
            var orgTickets = _context.Tickets
                .Where(t => t.Team.Branch.OrganizationId == orgId.Value);

            stats.TotalTickets = await orgTickets.CountAsync();
            stats.OpenTickets = await orgTickets.CountAsync(t => t.TicketState.Name == "Open");
            stats.ClosedTickets = await orgTickets.CountAsync(t => t.TicketState.Name == "Closed");
            stats.TotalCustomers = await _context.Customers
                .CountAsync(c => c.Tickets.Any(t => t.Team.Branch.OrganizationId == orgId.Value));
            stats.TotalAgents = await _context.SupportAgents
                .CountAsync(sa => sa.Team.Branch.OrganizationId == orgId.Value);
        }
        else if (IsCurrentUserAgent && teamId.HasValue)
        {
            // Agent sees only their stats
            var agentTickets = _context.Tickets
                .Where(t => t.SupportAgentId == UserContext.CurrentSupportAgent!.Id);

            stats.TotalTickets = await agentTickets.CountAsync();
            stats.OpenTickets = await agentTickets.CountAsync(t => t.TicketState.Name == "Open");
            stats.ClosedTickets = await agentTickets.CountAsync(t => t.TicketState.Name == "Closed");
            stats.MyTickets = stats.TotalTickets;
        }

        return stats;
    }
}

public class DashboardStatsViewModel
{
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int ClosedTickets { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalOrganizations { get; set; }
    public int TotalAgents { get; set; }
    public int MyTickets { get; set; }
}