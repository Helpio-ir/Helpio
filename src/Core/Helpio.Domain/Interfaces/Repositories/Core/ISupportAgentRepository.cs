using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Domain.Interfaces.Repositories;

namespace Helpio.Ir.Domain.Interfaces.Repositories.Core
{
    public interface ISupportAgentRepository : IRepository<SupportAgent>
    {
        Task<IEnumerable<SupportAgent>> GetByTeamIdAsync(int teamId);
        Task<IEnumerable<SupportAgent>> GetAvailableAgentsAsync();
        Task<IEnumerable<SupportAgent>> GetBySpecializationAsync(string specialization);
        Task<IEnumerable<SupportAgent>> GetBySupportLevelAsync(int supportLevel);
        Task<SupportAgent?> GetByAgentCodeAsync(string agentCode);
        Task<SupportAgent?> GetWithAssignedTicketsAsync(int agentId);
        Task<IEnumerable<SupportAgent>> GetAgentsWithLowWorkloadAsync(int maxTickets);
    }
}