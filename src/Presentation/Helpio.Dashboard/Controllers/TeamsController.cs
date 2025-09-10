using Helpio.Dashboard.Services;
using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Dashboard.Controllers
{
    public class TeamsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public TeamsController(ICurrentUserContext userContext, ApplicationDbContext context)
            : base(userContext)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var teams = await GetAccessibleTeamsAsync();
            return View(teams);
        }

        public async Task<IActionResult> Details(int id)
        {
            var team = await GetTeamByIdAsync(id);
            if (team == null || !CanAccessTeam(id))
            {
                return NotFound();
            }

            return View(team);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            ViewBag.Branches = await GetAccessibleBranchesAsync();
            return View(new Team());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Team team)
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                team.CreatedAt = DateTime.UtcNow;
                _context.Teams.Add(team);
                await _context.SaveChangesAsync();

                TempData["Success"] = "تیم با موفقیت ایجاد شد.";
                return RedirectToAction(nameof(Details), new { id = team.Id });
            }

            ViewBag.Branches = await GetAccessibleBranchesAsync();
            return View(team);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            var team = await _context.Teams.FindAsync(id);
            if (team == null || !CanAccessTeam(id))
            {
                return NotFound();
            }

            ViewBag.Branches = await GetAccessibleBranchesAsync();
            ViewBag.TeamAgents = await GetTeamAgentsAsync(id);
            return View(team);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Team team)
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            if (id != team.Id)
            {
                return BadRequest();
            }

            var existingTeam = await _context.Teams.FindAsync(id);
            if (existingTeam == null || !CanAccessTeam(id))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                existingTeam.Name = team.Name;
                existingTeam.Description = team.Description;
                existingTeam.BranchId = team.BranchId;
                existingTeam.TeamLeadId = team.TeamLeadId;
                existingTeam.SupervisorId = team.SupervisorId;
                existingTeam.IsActive = team.IsActive;
                existingTeam.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                TempData["Success"] = "اطلاعات تیم با موفقیت به‌روزرسانی شد.";
                return RedirectToAction(nameof(Details), new { id });
            }

            ViewBag.Branches = await GetAccessibleBranchesAsync();
            ViewBag.TeamAgents = await GetTeamAgentsAsync(id);
            return View(team);
        }

        private async Task<List<Team>> GetAccessibleTeamsAsync()
        {
            var query = _context.Teams
                .Include(t => t.Branch)
                    .ThenInclude(b => b.Organization)
                .Include(t => t.TeamLead)
                    .ThenInclude(tl => tl.User)
                .Include(t => t.SupportAgents)
                    .ThenInclude(sa => sa.User)
                .AsQueryable();

            if (IsCurrentUserAdmin)
            {
                // Admin sees all teams
                return await query.OrderBy(t => t.Branch.Organization.Name).ThenBy(t => t.Name).ToListAsync();
            }
            else if (IsCurrentUserManager && CurrentOrganizationId.HasValue)
            {
                // Manager sees teams in their organization
                query = query.Where(t => t.Branch.OrganizationId == CurrentOrganizationId.Value);
            }
            else if (IsCurrentUserAgent && CurrentTeamId.HasValue)
            {
                // Agent sees only their team
                query = query.Where(t => t.Id == CurrentTeamId.Value);
            }
            else
            {
                return new List<Team>();
            }

            return await query.OrderBy(t => t.Name).ToListAsync();
        }

        private async Task<Team?> GetTeamByIdAsync(int id)
        {
            return await _context.Teams
                .Include(t => t.Branch)
                    .ThenInclude(b => b.Organization)
                .Include(t => t.TeamLead)
                    .ThenInclude(tl => tl.User)
                .Include(t => t.Supervisor)
                    .ThenInclude(s => s.User)
                .Include(t => t.SupportAgents)
                    .ThenInclude(sa => sa.User)
                .Include(t => t.Tickets)
                    .ThenInclude(ticket => ticket.TicketState)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        private async Task<List<Branch>> GetAccessibleBranchesAsync()
        {
            var query = _context.Branches
                .Include(b => b.Organization)
                .AsQueryable();

            if (!IsCurrentUserAdmin && CurrentOrganizationId.HasValue)
            {
                query = query.Where(b => b.OrganizationId == CurrentOrganizationId.Value);
            }

            return await query.OrderBy(b => b.Organization.Name).ThenBy(b => b.Name).ToListAsync();
        }

        private async Task<List<SupportAgent>> GetTeamAgentsAsync(int teamId)
        {
            return await _context.SupportAgents
                .Include(sa => sa.User)
                .Where(sa => sa.TeamId == teamId)
                .OrderBy(sa => sa.User.FirstName)
                .ToListAsync();
        }
    }
}