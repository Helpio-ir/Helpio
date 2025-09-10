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
                .AsQueryable();

            query = ApplyAccessFilter(query);

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
                .Where(t => t.SupportAgent != null)
                .AsQueryable();

            query = ApplyAccessFilter(query);

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
            var query = _context.Tickets.AsQueryable();
            query = ApplyAccessFilter(query);

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
                .AsQueryable();

            query = ApplyAccessFilter(query);

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
            var query = _context.SupportAgents
                .Include(sa => sa.User)
                .Include(sa => sa.AssignedTickets)
                    .ThenInclude(t => t.TicketState)
                .AsQueryable();

            if (!IsCurrentUserAdmin && CurrentOrganizationId.HasValue)
            {
                query = query.Where(sa => sa.Team.Branch.OrganizationId == CurrentOrganizationId.Value);
            }

            var agents = await query.ToListAsync();

            var performance = agents.Select(agent => new AgentPerformanceItem
            {
                AgentName = $"{agent.User.FirstName} {agent.User.LastName}",
                TotalTickets = agent.AssignedTickets.Count,
                ClosedTickets = agent.AssignedTickets.Count(t => t.TicketState.Name == "Closed"),
                OpenTickets = agent.AssignedTickets.Count(t => t.TicketState.Name == "Open"),
                SuccessRate = agent.AssignedTickets.Count > 0
                    ? (double)agent.AssignedTickets.Count(t => t.TicketState.Name == "Closed") / agent.AssignedTickets.Count * 100
                    : 0
            }).ToList();

            return new AgentPerformanceViewModel
            {
                Agents = performance
            };
        }

        private IQueryable<T> ApplyAccessFilter<T>(IQueryable<T> query) where T : class
        {
            // This is a generic method - in real implementation, you'd need specific logic per entity type
            return query;
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