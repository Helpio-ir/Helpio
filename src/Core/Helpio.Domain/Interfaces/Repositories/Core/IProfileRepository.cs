using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Domain.Interfaces.Repositories;

namespace Helpio.Ir.Domain.Interfaces.Repositories.Core
{
    public interface IProfileRepository : IRepository<Profile>
    {
        Task<Profile?> GetBySupportAgentIdAsync(int supportAgentId);
        Task<IEnumerable<Profile>> GetBySkillsAsync(string skills);
    }
}