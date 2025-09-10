using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Dashboard.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly ApplicationDbContext _context;

        public OrganizationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Organization?> GetUserOrganizationAsync(int userId)
        {
            var supportAgent = await _context.SupportAgents
                .Include(sa => sa.Team)
                    .ThenInclude(t => t.Branch)
                        .ThenInclude(b => b.Organization)
                .FirstOrDefaultAsync(sa => sa.UserId == userId);

            return supportAgent?.Team?.Branch?.Organization;
        }

        public async Task<Team?> GetUserTeamAsync(int userId)
        {
            var supportAgent = await _context.SupportAgents
                .Include(sa => sa.Team)
                .FirstOrDefaultAsync(sa => sa.UserId == userId);

            return supportAgent?.Team;
        }

        public async Task<SupportAgent?> GetUserSupportAgentAsync(int userId)
        {
            return await _context.SupportAgents
                .Include(sa => sa.Team)
                    .ThenInclude(t => t.Branch)
                        .ThenInclude(b => b.Organization)
                .Include(sa => sa.Profile)
                .FirstOrDefaultAsync(sa => sa.UserId == userId);
        }

        public async Task<bool> IsUserInOrganizationAsync(int userId, int organizationId)
        {
            var userOrganization = await GetUserOrganizationAsync(userId);
            return userOrganization?.Id == organizationId;
        }

        public async Task<bool> IsUserInTeamAsync(int userId, int teamId)
        {
            var userTeam = await GetUserTeamAsync(userId);
            return userTeam?.Id == teamId;
        }

        public async Task<List<Organization>> GetUserAccessibleOrganizationsAsync(int userId)
        {
            // اگر کاربر Admin است، به همه سازمان‌ها دسترسی دارد
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return new List<Organization>();

            // بررسی نقش کاربر
            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .ToListAsync();

            if (userRoles.Contains("Admin"))
            {
                // Admin به همه سازمان‌ها دسترسی دارد
                return await _context.Organizations
                    .Where(o => o.IsActive)
                    .ToListAsync();
            }

            // کاربران عادی فقط به سازمان خودشان دسترسی دارند
            var userOrganization = await GetUserOrganizationAsync(userId);
            return userOrganization != null ? new List<Organization> { userOrganization } : new List<Organization>();
        }
    }
}