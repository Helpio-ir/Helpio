using Helpio.Dashboard.Services;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Dashboard.Controllers
{
    public class ReportsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ICurrentUserContext userContext, ApplicationDbContext context)
            : base(userContext)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var reports = new ReportsViewModel
            {
                TicketsByStatus = await GetTicketsByStatusAsync(),
                TicketsByAgent = await GetTicketsByAgentAsync(),
                CustomerSatisfaction = await GetCustomerSatisfactionAsync(),
                MonthlyTickets = await GetMonthlyTicketsAsync()
            };

            return View(reports);
        }

        public async Task<IActionResult> TicketReport()
        {
            var report = await GetDetailedTicketReportAsync();
            return View(report);
        }

        public async Task<IActionResult> AgentPerformance()
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            var report = await GetAgentPerformanceReportAsync();
            return View(report);
        }

        private async Task<Dictionary<string, int>> GetTicketsByStatusAsync()
        {
            var query = _context.Tickets
                .Include(t => t.TicketState)
                .Include(t => t.Team)
                    .ThenInclude(t => t.Branch)
                .AsQueryable();

            // Apply organization filter for non-admin users
            if (!IsCurrentUserAdmin && CurrentOrganizationId.HasValue)
            {
                query = query.Where(t => t.Team.Branch.OrganizationId == CurrentOrganizationId.Value);
            }

            return await query
                .GroupBy(t => t.TicketState.Name)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);
        }

        private async Task<Dictionary<string, int>> GetTicketsByAgentAsync()
        {
            var query = _context.Tickets
                .Include(t => t.SupportAgent)
                    .ThenInclude(sa => sa.User)
                .Include(t => t.Team)
                    .ThenInclude(t => t.Branch)
                .Where(t => t.SupportAgent != null)
                .AsQueryable();

            // Apply organization filter for non-admin users
            if (!IsCurrentUserAdmin && CurrentOrganizationId.HasValue)
            {
                query = query.Where(t => t.Team.Branch.OrganizationId == CurrentOrganizationId.Value);
            }

            return await query
                .GroupBy(t => t.SupportAgent!.User.FirstName + " " + t.SupportAgent.User.LastName)
                .Select(g => new { Agent = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Agent, x => x.Count);
        }

        private async Task<double> GetCustomerSatisfactionAsync()
        {
            // Placeholder - در واقعیت باید از جدول نظرسنجی استفاده شود
            return await Task.FromResult(4.2);
        }

        private async Task<Dictionary<string, int>> GetMonthlyTicketsAsync()
        {
            var query = _context.Tickets
                .Include(t => t.Team)
                    .ThenInclude(t => t.Branch)
                .AsQueryable();

            // Apply organization filter for non-admin users
            if (!IsCurrentUserAdmin && CurrentOrganizationId.HasValue)
            {
                query = query.Where(t => t.Team.Branch.OrganizationId == CurrentOrganizationId.Value);
            }

            var threeMonthsAgo = DateTime.UtcNow.AddMonths(-3);

            return await query
                .Where(t => t.CreatedAt >= threeMonthsAgo)
                .GroupBy(t => new { t.CreatedAt.Year, t.CreatedAt.Month })
                .Select(g => new
                {
                    Month = $"{g.Key.Year}/{g.Key.Month:00}",
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.Month, x => x.Count);
        }

        private async Task<TicketReportViewModel> GetDetailedTicketReportAsync()
        {
            var query = _context.Tickets
                .Include(t => t.Customer)
                .Include(t => t.TicketState)
                .Include(t => t.SupportAgent)
                    .ThenInclude(sa => sa.User)
                .Include(t => t.Team)
                    .ThenInclude(t => t.Branch)
                .AsQueryable();

            // Apply organization filter for non-admin users
            if (!IsCurrentUserAdmin && CurrentOrganizationId.HasValue)
            {
                query = query.Where(t => t.Team.Branch.OrganizationId == CurrentOrganizationId.Value);
            }

            var tickets = await query
                .OrderByDescending(t => t.CreatedAt)
                .Take(100)
                .ToListAsync();

            return new TicketReportViewModel
            {
                Tickets = tickets,
                TotalCount = await query.CountAsync(),
                AverageResolutionTime = await CalculateAverageResolutionTimeAsync(query)
            };
        }

        private async Task<AgentPerformanceViewModel> GetAgentPerformanceReportAsync()
        {
            // Build base query for agents with proper organization filtering
            var agentQuery = _context.SupportAgents
                .Include(sa => sa.User)
                .Include(sa => sa.Team)
                    .ThenInclude(t => t.Branch)
                .AsQueryable();

            if (!IsCurrentUserAdmin && CurrentOrganizationId.HasValue)
            {
                agentQuery = agentQuery.Where(sa => sa.Team != null && sa.Team.Branch.OrganizationId == CurrentOrganizationId.Value);
            }

            // Build base query for tickets with proper organization filtering
            var ticketQuery = _context.Tickets
                .Include(t => t.TicketState)
                .Include(t => t.SupportAgent)
                    .ThenInclude(sa => sa.User)
                .Include(t => t.Team)
                    .ThenInclude(t => t.Branch)
                .Where(t => t.SupportAgent != null)
                .AsQueryable();

            if (!IsCurrentUserAdmin && CurrentOrganizationId.HasValue)
            {
                ticketQuery = ticketQuery.Where(t => t.Team.Branch.OrganizationId == CurrentOrganizationId.Value);
            }

            // Get agent performance data using a single efficient query
            var agentPerformanceData = await (from agent in agentQuery
                                            join ticket in ticketQuery on agent.Id equals ticket.SupportAgentId into agentTickets
                                            select new
                                            {
                                                Agent = agent,
                                                Tickets = agentTickets.ToList()
                                            }).ToListAsync();

            var performance = agentPerformanceData.Select(data => new AgentPerformanceItem
            {
                AgentName = $"{data.Agent.User.FirstName} {data.Agent.User.LastName}",
                TotalTickets = data.Tickets.Count,
                ClosedTickets = data.Tickets.Count(t => t.TicketState.Name == "Closed"),
                OpenTickets = data.Tickets.Count(t => t.TicketState.Name == "Open"),
                SuccessRate = data.Tickets.Count > 0
                    ? (double)data.Tickets.Count(t => t.TicketState.Name == "Closed") / data.Tickets.Count * 100
                    : 0
            }).ToList();

            return new AgentPerformanceViewModel
            {
                Agents = performance
            };
        }

        private async Task<double> CalculateAverageResolutionTimeAsync(IQueryable<Helpio.Ir.Domain.Entities.Ticketing.Ticket> query)
        {
            var closedTickets = await query
                .Where(t => t.TicketState.Name == "Closed" && t.UpdatedAt.HasValue)
                .Select(t => new { Created = t.CreatedAt, Updated = t.UpdatedAt!.Value })
                .ToListAsync();

            if (!closedTickets.Any())
                return 0;

            var averageHours = closedTickets
                .Select(t => (t.Updated - t.Created).TotalHours)
                .Average();

            return Math.Round(averageHours, 1);
        }
    }

    public class ReportsViewModel
    {
        public Dictionary<string, int> TicketsByStatus { get; set; } = new();
        public Dictionary<string, int> TicketsByAgent { get; set; } = new();
        public double CustomerSatisfaction { get; set; }
        public Dictionary<string, int> MonthlyTickets { get; set; } = new();
    }

    public class TicketReportViewModel
    {
        public List<Helpio.Ir.Domain.Entities.Ticketing.Ticket> Tickets { get; set; } = new();
        public int TotalCount { get; set; }
        public double AverageResolutionTime { get; set; }
    }

    public class AgentPerformanceViewModel
    {
        public List<AgentPerformanceItem> Agents { get; set; } = new();
    }

    public class AgentPerformanceItem
    {
        public string AgentName { get; set; } = string.Empty;
        public int TotalTickets { get; set; }
        public int ClosedTickets { get; set; }
        public int OpenTickets { get; set; }
        public double SuccessRate { get; set; }
    }
}