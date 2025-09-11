using Helpio.Dashboard.Services;
using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Dashboard.Controllers
{
    public class SupportAgentsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public SupportAgentsController(ICurrentUserContext userContext, ApplicationDbContext context)
            : base(userContext)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var agents = await GetAccessibleAgentsAsync();
            return View(agents);
        }

        public async Task<IActionResult> Details(int id)
        {
            var agent = await GetAgentByIdAsync(id);
            if (agent == null || !CanAccessAgent(agent))
            {
                return NotFound();
            }

            return View(agent);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            ViewBag.Users = await GetAvailableUsersAsync();
            ViewBag.Teams = await GetAccessibleTeamsAsync();
            // ViewBag.Profiles = await GetAccessibleProfilesAsync();  // Remove for now

            return View(new SupportAgent());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupportAgent agent)
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            // Remove validation for navigation properties
            ModelState.Remove(nameof(agent.Team));
            ModelState.Remove(nameof(agent.User));
            ModelState.Remove(nameof(agent.Profile));
            ModelState.Remove(nameof(agent.ManagedTeams));
            ModelState.Remove(nameof(agent.SupervisedTeams));
            ModelState.Remove(nameof(agent.AssignedTickets));
            ModelState.Remove(nameof(agent.Notes));

            // Set default ProfileId
            agent.ProfileId = 1; // Set to a default profile or create one

            // Generate agent code if not provided
            if (string.IsNullOrEmpty(agent.AgentCode))
            {
                agent.AgentCode = await GenerateAgentCodeAsync();
            }

            // Validate unique agent code
            if (await _context.SupportAgents.AnyAsync(sa => sa.AgentCode == agent.AgentCode))
            {
                ModelState.AddModelError(nameof(agent.AgentCode), "کد کارشناس تکراری است.");
            }

            // Validate user is not already an agent
            if (await _context.SupportAgents.AnyAsync(sa => sa.UserId == agent.UserId))
            {
                ModelState.AddModelError(nameof(agent.UserId), "این کاربر قبلاً به عنوان کارشناس ثبت شده است.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    agent.CreatedAt = DateTime.UtcNow;
                    agent.HireDate = agent.HireDate == default ? DateTime.UtcNow : agent.HireDate;

                    _context.SupportAgents.Add(agent);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "کارشناس با موفقیت ایجاد شد.";
                    return RedirectToAction(nameof(Details), new { id = agent.Id });
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "خطا در ایجاد کارشناس. لطفا مجدداً تلاش کنید.";
                    ModelState.AddModelError("", $"خطا: {ex.Message}");
                }
            }

            ViewBag.Users = await GetAvailableUsersAsync();
            ViewBag.Teams = await GetAccessibleTeamsAsync();
            // ViewBag.Profiles = await GetAccessibleProfilesAsync();  // Remove for now

            return View(agent);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            var agent = await GetAgentByIdAsync(id);
            if (agent == null || !CanAccessAgent(agent))
            {
                return NotFound();
            }

            ViewBag.Users = await GetAvailableUsersAsync(agent.UserId);
            ViewBag.Teams = await GetAccessibleTeamsAsync();
            // ViewBag.Profiles = await GetAccessibleProfilesAsync();  // Remove for now

            return View(agent);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SupportAgent agent)
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            if (id != agent.Id)
            {
                return BadRequest();
            }

            var existingAgent = await _context.SupportAgents.FindAsync(id);
            if (existingAgent == null || !CanAccessAgent(existingAgent))
            {
                return NotFound();
            }

            // Remove validation for navigation properties
            ModelState.Remove(nameof(agent.Team));
            ModelState.Remove(nameof(agent.User));
            ModelState.Remove(nameof(agent.Profile));
            ModelState.Remove(nameof(agent.ManagedTeams));
            ModelState.Remove(nameof(agent.SupervisedTeams));
            ModelState.Remove(nameof(agent.AssignedTickets));
            ModelState.Remove(nameof(agent.Notes));

            // Validate unique agent code (excluding current agent)
            if (await _context.SupportAgents.AnyAsync(sa => sa.AgentCode == agent.AgentCode && sa.Id != id))
            {
                ModelState.AddModelError(nameof(agent.AgentCode), "کد کارشناس تکراری است.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingAgent.TeamId = agent.TeamId;
                    // existingAgent.ProfileId = agent.ProfileId;  // Skip profile for now
                    existingAgent.AgentCode = agent.AgentCode;
                    existingAgent.Department = agent.Department;
                    existingAgent.Position = agent.Position;
                    existingAgent.Specialization = agent.Specialization;
                    existingAgent.SupportLevel = agent.SupportLevel;
                    existingAgent.Salary = agent.Salary;
                    existingAgent.IsActive = agent.IsActive;
                    existingAgent.IsAvailable = agent.IsAvailable;
                    existingAgent.MaxConcurrentTickets = agent.MaxConcurrentTickets;
                    existingAgent.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "اطلاعات کارشناس با موفقیت به‌روزرسانی شد.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await AgentExistsAsync(agent.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.Users = await GetAvailableUsersAsync(agent.UserId);
            ViewBag.Teams = await GetAccessibleTeamsAsync();
            // ViewBag.Profiles = await GetAccessibleProfilesAsync();  // Remove for now

            return View(agent);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsCurrentUserAdmin)
            {
                return Forbid("فقط مدیران کل می‌توانند کارشناسان را حذف کنند.");
            }

            var agent = await GetAgentByIdAsync(id);
            if (agent == null || !CanAccessAgent(agent))
            {
                return NotFound();
            }

            // Check if agent has assigned tickets
            var hasTickets = await _context.Tickets.AnyAsync(t => t.SupportAgentId == id);
            if (hasTickets)
            {
                TempData["Error"] = "امکان حذف کارشناس وجود ندارد. این کارشناس دارای تیکت‌های مرتبط است.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Check if agent is team lead or supervisor
            var isTeamLead = await _context.Teams.AnyAsync(t => t.TeamLeadId == id);
            var isSupervisor = await _context.Teams.AnyAsync(t => t.SupervisorId == id);

            if (isTeamLead || isSupervisor)
            {
                TempData["Error"] = "امکان حذف کارشناس وجود ندارد. این کارشناس تیم لید یا سرپرست تیمی است.";
                return RedirectToAction(nameof(Details), new { id });
            }

            _context.SupportAgents.Remove(agent);
            await _context.SaveChangesAsync();

            TempData["Success"] = "کارشناس با موفقیت حذف شد.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<List<SupportAgent>> GetAccessibleAgentsAsync()
        {
            var query = _context.SupportAgents
                .Include(sa => sa.User)
                .Include(sa => sa.Team)
                    .ThenInclude(t => t.Branch)
                        .ThenInclude(b => b.Organization)
                .Include(sa => sa.Profile)
                .Include(sa => sa.AssignedTickets)
                    .ThenInclude(t => t.TicketState)
                .AsQueryable();

            if (IsCurrentUserAdmin)
            {
                // Admin sees all agents
                return await query.OrderBy(sa => sa.User.FirstName).ThenBy(sa => sa.User.LastName).ToListAsync();
            }
            else if (IsCurrentUserManager && CurrentOrganizationId.HasValue)
            {
                // Manager sees agents in their organization
                query = query.Where(sa => sa.Team == null || sa.Team.Branch.OrganizationId == CurrentOrganizationId.Value);
            }
            else if (IsCurrentUserAgent && CurrentTeamId.HasValue)
            {
                // Agent sees only team members
                query = query.Where(sa => sa.TeamId == CurrentTeamId.Value);
            }
            else
            {
                return new List<SupportAgent>();
            }

            return await query.OrderBy(sa => sa.User.FirstName).ThenBy(sa => sa.User.LastName).ToListAsync();
        }

        private async Task<SupportAgent?> GetAgentByIdAsync(int id)
        {
            return await _context.SupportAgents
                .Include(sa => sa.User)
                .Include(sa => sa.Team)
                    .ThenInclude(t => t.Branch)
                        .ThenInclude(b => b.Organization)
                .Include(sa => sa.Profile)
                .Include(sa => sa.AssignedTickets)
                    .ThenInclude(t => t.TicketState)
                .Include(sa => sa.Notes)
                .FirstOrDefaultAsync(sa => sa.Id == id);
        }

        private async Task<List<User>> GetAvailableUsersAsync(int? excludeUserId = null)
        {
            var query = _context.Users
                .Where(u => !_context.SupportAgents.Any(sa => sa.UserId == u.Id))
                .AsQueryable();

            if (excludeUserId.HasValue)
            {
                query = query.Where(u => u.Id == excludeUserId.Value || !_context.SupportAgents.Any(sa => sa.UserId == u.Id));
            }

            return await query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName).ToListAsync();
        }

        private async Task<List<Team>> GetAccessibleTeamsAsync()
        {
            var query = _context.Teams
                .Include(t => t.Branch)
                    .ThenInclude(b => b.Organization)
                .Where(t => t.IsActive)
                .AsQueryable();

            if (!IsCurrentUserAdmin && CurrentOrganizationId.HasValue)
            {
                query = query.Where(t => t.Branch.OrganizationId == CurrentOrganizationId.Value);
            }

            return await query.OrderBy(t => t.Name).ToListAsync();
        }

        private async Task<List<Profile>> GetAccessibleProfilesAsync()
        {
            return await _context.Profiles
                .OrderBy(p => p.Id)
                .ToListAsync();
        }

        private bool CanAccessAgent(SupportAgent agent)
        {
            if (IsCurrentUserAdmin) return true;

            if (IsCurrentUserManager && CurrentOrganizationId.HasValue)
            {
                return agent.Team?.Branch?.OrganizationId == CurrentOrganizationId.Value;
            }

            if (IsCurrentUserAgent)
            {
                return agent.TeamId == CurrentTeamId;
            }

            return false;
        }

        private async Task<bool> AgentExistsAsync(int id)
        {
            return await _context.SupportAgents.AnyAsync(e => e.Id == id);
        }

        private async Task<string> GenerateAgentCodeAsync()
        {
            var today = DateTime.Today;
            var prefix = "AG" + today.ToString("yyyyMM");

            var lastCode = await _context.SupportAgents
                .Where(sa => sa.AgentCode.StartsWith(prefix))
                .OrderByDescending(sa => sa.AgentCode)
                .Select(sa => sa.AgentCode)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (!string.IsNullOrEmpty(lastCode) && lastCode.Length > prefix.Length)
            {
                var numberPart = lastCode.Substring(prefix.Length);
                if (int.TryParse(numberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return prefix + nextNumber.ToString("0000");
        }
    }
}