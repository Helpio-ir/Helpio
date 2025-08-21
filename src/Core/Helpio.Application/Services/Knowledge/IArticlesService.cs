using Helpio.Ir.Application.DTOs.Knowledge;
using Helpio.Ir.Application.DTOs.Common;

namespace Helpio.Ir.Application.Services.Knowledge
{
    public interface IArticlesService
    {
        Task<ArticlesDto?> GetByIdAsync(int id);
        Task<PaginatedResult<ArticlesDto>> GetArticlesAsync(PaginationRequest request);
        Task<ArticlesDto> CreateAsync(CreateArticlesDto createDto);
        Task<ArticlesDto> UpdateAsync(int id, UpdateArticlesDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<ArticlesDto>> GetByOrganizationIdAsync(int organizationId);
        Task<IEnumerable<ArticlesDto>> GetPublishedArticlesAsync();
        Task<IEnumerable<ArticlesDto>> GetByAuthorIdAsync(int authorId);
        Task<IEnumerable<ArticlesDto>> SearchByTagsAsync(string tags);
        Task<IEnumerable<ArticlesDto>> GetMostViewedArticlesAsync(int count);
        Task<ArticlesDto?> IncrementViewCountAsync(int articleId);
        Task<bool> PublishArticleAsync(int articleId);
        Task<bool> UnpublishArticleAsync(int articleId);
    }
}