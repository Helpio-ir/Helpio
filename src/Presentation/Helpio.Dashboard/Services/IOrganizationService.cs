using Helpio.Ir.Domain.Entities.Core;

namespace Helpio.Dashboard.Services
{
    public interface IOrganizationService
    {
        Task<Organization?> GetUserOrganizationAsync(int userId);
        Task<Team?> GetUserTeamAsync(int userId);
        Task<SupportAgent?> GetUserSupportAgentAsync(int userId);
        Task<bool> IsUserInOrganizationAsync(int userId, int organizationId);
        Task<bool> IsUserInTeamAsync(int userId, int teamId);
        Task<List<Organization>> GetUserAccessibleOrganizationsAsync(int userId);
    }
}