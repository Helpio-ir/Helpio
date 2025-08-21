using Helpio.Ir.Domain.Entities.Knowledge;
using Helpio.Ir.Domain.Interfaces.Repositories;

namespace Helpio.Ir.Domain.Interfaces.Repositories.Knowledge
{
    public interface ICannedResponseRepository : IRepository<CannedResponse>
    {
        Task<IEnumerable<CannedResponse>> GetByOrganizationIdAsync(int organizationId);
        Task<IEnumerable<CannedResponse>> GetActiveResponsesAsync();
        Task<IEnumerable<CannedResponse>> SearchByTagsAsync(string tags);
        Task<IEnumerable<CannedResponse>> GetMostUsedResponsesAsync(int count);
        Task<CannedResponse?> GetByNameAsync(string name);
    }
}