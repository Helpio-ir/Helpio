using Helpio.Ir.Domain.Entities.Knowledge;
using Helpio.Ir.Domain.Interfaces.Repositories;

namespace Helpio.Ir.Domain.Interfaces.Repositories.Knowledge
{
    public interface IArticlesRepository : IRepository<Articles>
    {
        Task<IEnumerable<Articles>> GetByOrganizationIdAsync(int organizationId);
        Task<IEnumerable<Articles>> GetPublishedArticlesAsync();
        Task<IEnumerable<Articles>> GetByAuthorIdAsync(int authorId);
        Task<IEnumerable<Articles>> SearchByTagsAsync(string tags);
        Task<IEnumerable<Articles>> GetMostViewedArticlesAsync(int count);
        Task<Articles?> IncrementViewCountAsync(int articleId);
    }
}