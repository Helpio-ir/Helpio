using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Domain.Interfaces.Repositories;

namespace Helpio.Ir.Domain.Interfaces.Repositories.Core
{
    public interface ITeamRepository : IRepository<Team>
    {
        Task<IEnumerable<Team>> GetByBranchIdAsync(int branchId);
        Task<IEnumerable<Team>> GetActiveTeamsAsync();
        Task<Team?> GetWithSupportAgentsAsync(int teamId);
        Task<IEnumerable<Team>> GetTeamsByManagerAsync(int managerId);
    }
}