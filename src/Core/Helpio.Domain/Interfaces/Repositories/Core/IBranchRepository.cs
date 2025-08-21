using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Domain.Interfaces.Repositories;

namespace Helpio.Ir.Domain.Interfaces.Repositories.Core
{
    public interface IBranchRepository : IRepository<Branch>
    {
        Task<IEnumerable<Branch>> GetByOrganizationIdAsync(int organizationId);
        Task<IEnumerable<Branch>> GetActiveBranchesAsync();
        Task<Branch?> GetWithTeamsAsync(int branchId);
    }
}