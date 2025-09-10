using Helpio.Ir.Domain.Entities.Core;

namespace Helpio.Dashboard.Services
{
    public interface ICurrentUserContext
    {
        int? UserId { get; }
        string? UserEmail { get; }
        string? UserFullName { get; }
        Organization? CurrentOrganization { get; set; }
        Team? CurrentTeam { get; set; }
        SupportAgent? CurrentSupportAgent { get; set; }
        List<string> UserRoles { get; set; }

        Task LoadUserContextAsync();
        bool HasRole(string roleName);
        bool IsAdmin { get; }
        bool IsManager { get; }
        bool IsAgent { get; }
        bool CanAccessOrganization(int organizationId);
        bool CanAccessTeam(int teamId);
    }
}