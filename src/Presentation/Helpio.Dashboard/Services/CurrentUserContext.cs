using System.Security.Claims;
using Helpio.Ir.Domain.Entities.Core;
using Microsoft.AspNetCore.Identity;

namespace Helpio.Dashboard.Services
{
    public class CurrentUserContext : ICurrentUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;
        private readonly IOrganizationService _organizationService;
        private readonly ILogger<CurrentUserContext> _logger;

        public CurrentUserContext(
            IHttpContextAccessor httpContextAccessor,
            UserManager<User> userManager,
            IOrganizationService organizationService,
            ILogger<CurrentUserContext> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _organizationService = organizationService;
            _logger = logger;
        }

        public int? UserId
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return int.TryParse(userIdClaim, out var userId) ? userId : null;
            }
        }

        public string? UserEmail => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

        public string? UserFullName => _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        public Organization? CurrentOrganization { get; set; }
        public Team? CurrentTeam { get; set; }
        public SupportAgent? CurrentSupportAgent { get; set; }
        public List<string> UserRoles { get; set; } = new();

        public bool IsAdmin => HasRole("Admin");
        public bool IsManager => HasRole("Manager");
        public bool IsAgent => HasRole("Agent");

        public async Task LoadUserContextAsync()
        {
            try
            {
                if (UserId == null) return;

                var user = await _userManager.FindByIdAsync(UserId.ToString()!);
                if (user == null) return;

                // بارگذاری نقش‌های کاربر
                UserRoles = (await _userManager.GetRolesAsync(user)).ToList();

                // بارگذاری اطلاعات سازمان و تیم کاربر
                CurrentSupportAgent = await _organizationService.GetUserSupportAgentAsync(UserId.Value);
                CurrentOrganization = await _organizationService.GetUserOrganizationAsync(UserId.Value);
                CurrentTeam = await _organizationService.GetUserTeamAsync(UserId.Value);

                _logger.LogInformation(
                    "User context loaded for {UserEmail}. Organization: {OrganizationName}, Team: {TeamName}, Roles: {Roles}",
                    UserEmail,
                    CurrentOrganization?.Name ?? "None",
                    CurrentTeam?.Name ?? "None",
                    string.Join(", ", UserRoles));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user context for user {UserId}", UserId);
            }
        }

        public bool HasRole(string roleName)
        {
            return UserRoles.Contains(roleName);
        }

        public bool CanAccessOrganization(int organizationId)
        {
            // Admin به همه سازمان‌ها دسترسی دارد
            if (IsAdmin) return true;

            // کاربران عادی فقط به سازمان خودشان دسترسی دارند
            return CurrentOrganization?.Id == organizationId;
        }

        public bool CanAccessTeam(int teamId)
        {
            // Admin به همه تیم‌ها دسترسی دارد
            if (IsAdmin) return true;

            // Manager به همه تیم‌های سازمان خودش دسترسی دارد
            if (IsManager && CurrentOrganization != null)
            {
                // اینجا باید چک کنیم که آیا تیم مورد نظر در همان سازمان کاربر است یا نه
                // برای سادگی فعلاً فقط به تیم خودش دسترسی دارد
                return CurrentTeam?.Id == teamId;
            }

            // Agent فقط به تیم خودش دسترسی دارد
            return CurrentTeam?.Id == teamId;
        }
    }
}