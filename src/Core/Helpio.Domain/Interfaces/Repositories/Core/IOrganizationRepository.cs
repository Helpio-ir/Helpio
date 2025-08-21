using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Domain.Interfaces.Repositories;

namespace Helpio.Ir.Domain.Interfaces.Repositories.Core
{
    public interface IOrganizationRepository : IRepository<Organization>
    {
        Task<IEnumerable<Organization>> GetActiveOrganizationsAsync();
        Task<Organization?> GetWithBranchesAsync(int organizationId);
        Task<Organization?> GetWithTicketCategoriesAsync(int organizationId);
    }
}