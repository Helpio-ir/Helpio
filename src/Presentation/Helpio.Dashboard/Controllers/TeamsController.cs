using Helpio.Dashboard.Models;
using Helpio.Dashboard.Services;
using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Dashboard.Controllers
{
    public class QuickCreateAgentRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Position { get; set; }
        public string? Specialization { get; set; }
        public int SupportLevel { get; set; } = 1;
        public int MaxConcurrentTickets { get; set; } = 10;
        public bool AddToTeam { get; set; } = false;
        public int TeamId { get; set; }
    }

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
            ViewBag.SupportAgents = await GetAvailableAgentsAsync();

            return View(new CreateTeamDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTeamDto dto)
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                // Map DTO to Entity
                var team = new Team
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    BranchId = dto.BranchId,
                    TeamLeadId = dto.TeamLeadId,
                    SupervisorId = dto.SupervisorId,
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Teams.Add(team);
                await _context.SaveChangesAsync();

                TempData["Success"] = "تیم با موفقیت ایجاد شد.";
                return RedirectToAction(nameof(Details), new { id = team.Id });
            }
            else
            {
                // Debug ModelState if not valid
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                    .ToList();

                // For debugging - remove in production
                TempData["Debug"] = string.Join("; ", errors.Select(e => $"{e.Field}: {string.Join(", ", e.Errors)}"));
            }

            ViewBag.Branches = await GetAccessibleBranchesAsync();
            ViewBag.SupportAgents = await GetAvailableAgentsAsync();

            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            var team = await GetTeamByIdAsync(id);
            if (team == null || !CanAccessTeam(id))
            {
                return NotFound();
            }

            ViewBag.Branches = await GetAccessibleBranchesAsync();
            ViewBag.SupportAgents = await GetAvailableAgentsAsync();
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
                try
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
                catch (DbUpdateConcurrencyException)
                {
                    if (!await TeamExistsAsync(team.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.Branches = await GetAccessibleBranchesAsync();
            ViewBag.SupportAgents = await GetAvailableAgentsAsync();
            return View(team);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsCurrentUserAdmin)
            {
                return Forbid("فقط مدیران کل می‌توانند تیم‌ها را حذف کنند.");
            }

            var team = await GetTeamByIdAsync(id);
            if (team == null || !CanAccessTeam(id))
            {
                return NotFound();
            }

            // Check if team has tickets
            var hasTickets = await _context.Tickets.AnyAsync(t => t.TeamId == id);
            if (hasTickets)
            {
                TempData["Error"] = "امکان حذف تیم وجود ندارد. این تیم دارای تیکت‌های مرتبط است.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Check if team has agents
            var hasAgents = await _context.SupportAgents.AnyAsync(sa => sa.TeamId == id);
            if (hasAgents)
            {
                TempData["Error"] = "امکان حذف تیم وجود ندارد. ابتدا کارشناسان را از تیم خارج کنید.";
                return RedirectToAction(nameof(Details), new { id });
            }

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();

            TempData["Success"] = "تیم با موفقیت حذف شد.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(int teamId, int agentId)
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            var team = await GetTeamByIdAsync(teamId);
            if (team == null || !CanAccessTeam(teamId))
            {
                return NotFound();
            }

            var agent = await _context.SupportAgents
                .Include(sa => sa.User)
                .Include(sa => sa.Team)
                    .ThenInclude(t => t.Branch)
                .FirstOrDefaultAsync(sa => sa.Id == agentId);

            if (agent == null)
            {
                TempData["Error"] = "کارشناس مورد نظر یافت نشد.";
                return RedirectToAction(nameof(Edit), new { id = teamId });
            }

            // Check if agent is already in this team
            if (agent.TeamId.HasValue && agent.TeamId.Value == teamId)
            {
                TempData["Warning"] = $"کارشناس {agent.User.FirstName} {agent.User.LastName} قبلاً عضو این تیم است.";
                return RedirectToAction(nameof(Edit), new { id = teamId });
            }

            // Check organization access for non-admin users
            if (!IsCurrentUserAdmin && CurrentOrganizationId.HasValue)
            {
                if (agent.Team?.Branch?.OrganizationId != CurrentOrganizationId.Value)
                {
                    TempData["Error"] = "شما مجاز به انتقال این کارشناس نیستید.";
                    return RedirectToAction(nameof(Edit), new { id = teamId });
                }
            }

            // Store previous team info for message
            var previousTeam = agent.Team?.Name;

            // Transfer agent to new team
            agent.TeamId = teamId;
            agent.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(previousTeam))
            {
                TempData["Success"] = $"کارشناس {agent.User.FirstName} {agent.User.LastName} از تیم '{previousTeam}' به این تیم منتقل شد.";
            }
            else
            {
                TempData["Success"] = $"کارشناس {agent.User.FirstName} {agent.User.LastName} با موفقیت به تیم اضافه شد.";
            }

            return RedirectToAction(nameof(Edit), new { id = teamId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int teamId, int agentId)
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            var team = await GetTeamByIdAsync(teamId);
            if (team == null || !CanAccessTeam(teamId))
            {
                return NotFound();
            }

            var agent = await _context.SupportAgents
                .Include(sa => sa.User)
                .FirstOrDefaultAsync(sa => sa.Id == agentId && sa.TeamId == teamId);

            if (agent == null)
            {
                TempData["Error"] = "کارشناس مورد نظر در این تیم یافت نشد.";
                return RedirectToAction(nameof(Edit), new { id = teamId });
            }

            // Check if agent is team lead or supervisor
            if (team.TeamLeadId == agentId)
            {
                TempData["Warning"] = $"کارشناس {agent.User.FirstName} {agent.User.LastName} تیم لید این تیم است. ابتدا تیم لید را تغییر دهید.";
                return RedirectToAction(nameof(Edit), new { id = teamId });
            }

            if (team.SupervisorId == agentId)
            {
                TempData["Warning"] = $"کارشناس {agent.User.FirstName} {agent.User.LastName} سرپرست این تیم است. ابتدا سرپرست را تغییر دهید.";
                return RedirectToAction(nameof(Edit), new { id = teamId });
            }

            // Check if agent has open tickets
            var hasOpenTickets = await _context.Tickets.AnyAsync(t =>
                t.SupportAgentId == agentId &&
                t.TicketState.Name == "Open");

            if (hasOpenTickets)
            {
                TempData["Warning"] = $"کارشناس {agent.User.FirstName} {agent.User.LastName} دارای تیکت‌های باز است. ابتدا تیکت‌ها را به کارشناس دیگری منتقل کنید.";
                return RedirectToAction(nameof(Edit), new { id = teamId });
            }

            // Remove agent from team
            agent.TeamId = null;
            agent.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"کارشناس {agent.User.FirstName} {agent.User.LastName} از تیم حذف شد.";
            return RedirectToAction(nameof(Edit), new { id = teamId });
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableAgents(int teamId, int? branchId = null)
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Json(new { error = "دسترسی غیرمجاز" });
            }

            try
            {
                var query = _context.SupportAgents
                    .Include(sa => sa.User)
                    .Include(sa => sa.Team)
                        .ThenInclude(t => t.Branch)
                            .ThenInclude(b => b.Organization)
                    .Where(sa => sa.IsActive) // Only active agents
                    .AsQueryable();

                // Filter by organization for non-admin users
                if (!IsCurrentUserAdmin && CurrentOrganizationId.HasValue)
                {
                    query = query.Where(sa =>
                        sa.Team == null ||
                        sa.Team.Branch.OrganizationId == CurrentOrganizationId.Value
                    );
                }

                // Filter by branch if specified
                if (branchId.HasValue)
                {
                    query = query.Where(sa =>
                        sa.Team == null ||
                        sa.Team.BranchId == branchId.Value
                    );
                }

                var agents = await query
                    .OrderBy(sa => sa.User.FirstName)
                    .ThenBy(sa => sa.User.LastName)
                    .ToListAsync();

                var result = agents.Select(sa => new
                {
                    id = sa.Id,
                    name = $"{sa.User.FirstName} {sa.User.LastName}",
                    agentCode = sa.AgentCode,
                    position = sa.Position ?? "نامشخص",
                    currentTeam = sa.Team?.Name,
                    currentTeamId = sa.TeamId,
                    isInCurrentTeam = sa.TeamId == teamId,
                    // Allow all agents to be selectable for team transfer
                    isAvailable = true, // Changed: All agents are selectable
                    specialization = sa.Specialization,
                    supportLevel = sa.SupportLevel,
                    // Add status info for UI
                    canTransfer = sa.TeamId.HasValue && sa.TeamId != teamId,
                    isFree = !sa.TeamId.HasValue
                }).ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { error = $"خطا در بارگذاری کارشناسان: {ex.Message}" });
            }
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
                .Include(t => t.Tickets)
                    .ThenInclude(ticket => ticket.TicketState)
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

        private async Task<List<SupportAgent>> GetAvailableAgentsAsync()
        {
            var query = _context.SupportAgents
                .Include(sa => sa.User)
                .Include(sa => sa.Team)
                .AsQueryable();

            if (!IsCurrentUserAdmin && CurrentOrganizationId.HasValue)
            {
                query = query.Where(sa => sa.Team.Branch.OrganizationId == CurrentOrganizationId.Value);
            }

            return await query.OrderBy(sa => sa.User.FirstName).ToListAsync();
        }

        private bool CanAccessTeam(int teamId)
        {
            if (IsCurrentUserAdmin) return true;

            if (IsCurrentUserManager && CurrentOrganizationId.HasValue)
            {
                var team = _context.Teams.Include(t => t.Branch).FirstOrDefault(t => t.Id == teamId);
                return team?.Branch?.OrganizationId == CurrentOrganizationId.Value;
            }

            if (IsCurrentUserAgent)
            {
                return teamId == CurrentTeamId;
            }

            return false;
        }

        [HttpPost]
        public async Task<IActionResult> QuickCreateAgent([FromBody] QuickCreateAgentRequest request)
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Json(new { success = false, message = "دسترسی غیرمجاز" });
            }

            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(request.FirstName) ||
                    string.IsNullOrWhiteSpace(request.LastName) ||
                    string.IsNullOrWhiteSpace(request.Email))
                {
                    return Json(new { success = false, message = "فیلدهای اجباری نمی‌توانند خالی باشند." });
                }

                // Check if email already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

                User user;
                if (existingUser != null)
                {
                    // Check if user is already an agent
                    var existingAgent = await _context.SupportAgents
                        .FirstOrDefaultAsync(sa => sa.UserId == existingUser.Id);

                    if (existingAgent != null)
                    {
                        return Json(new { success = false, message = "این ایمیل قبلاً برای کارشناس دیگری استفاده شده است." });
                    }

                    user = existingUser;
                }
                else
                {
                    // Create new user
                    user = new User
                    {
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        Email = request.Email,
                        PhoneNumber = request.PhoneNumber,
                        UserName = request.Email,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }

                // Create a new Profile first
                var profile = new Profile
                {
                    Bio = "پروفایل جدید ایجاد شده",
                    Skills = "",
                    Avatar = "default.png",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Profiles.Add(profile);
                await _context.SaveChangesAsync();

                // Generate agent code
                var agentCode = await GenerateAgentCodeAsync();

                // Create support agent
                var agent = new SupportAgent
                {
                    UserId = user.Id,
                    AgentCode = agentCode,
                    Position = request.Position ?? "کارشناس",
                    Specialization = request.Specialization,
                    SupportLevel = request.SupportLevel,
                    MaxConcurrentTickets = request.MaxConcurrentTickets,
                    IsActive = true,
                    IsAvailable = true,
                    HireDate = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    ProfileId = profile.Id, // Use the newly created profile ID
                    Department = "پشتیبانی", // Default department
                    Salary = 0 // Default salary
                };

                // Add to team if requested
                if (request.AddToTeam)
                {
                    agent.TeamId = request.TeamId;
                }

                _context.SupportAgents.Add(agent);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "کارشناس با موفقیت ایجاد شد.",
                    agentId = agent.Id,
                    agentCode = agent.AgentCode
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا در ایجاد کارشناس: {ex.Message}" });
            }
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

        private async Task<bool> TeamExistsAsync(int id)
        {
            return await _context.Teams.AnyAsync(e => e.Id == id);
        }
    }
}