using Helpio.Ir.Domain.Entities.Knowledge;
using Helpio.Ir.Domain.Interfaces.Repositories.Knowledge;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Ir.Infrastructure.Repositories.Knowledge
{
    public class ArticlesRepository : Repository<Articles>, IArticlesRepository
    {
        public ArticlesRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Articles>> GetByOrganizationIdAsync(int organizationId)
        {
            return await _dbSet
                .Where(a => a.OrganizationId == organizationId && !a.IsDeleted)
                .Include(a => a.Author)
                .OrderByDescending(a => a.PublishedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Articles>> GetPublishedArticlesAsync()
        {
            return await _dbSet
                .Where(a => a.IsPublished && a.IsActive && !a.IsDeleted)
                .Include(a => a.Author)
                .Include(a => a.Organization)
                .OrderByDescending(a => a.PublishedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Articles>> GetByAuthorIdAsync(int authorId)
        {
            return await _dbSet
                .Where(a => a.AuthorId == authorId && !a.IsDeleted)
                .Include(a => a.Organization)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Articles>> SearchByTagsAsync(string tags)
        {
            return await _dbSet
                .Where(a => a.Tags != null && a.Tags.Contains(tags) &&
                           a.IsPublished && a.IsActive && !a.IsDeleted)
                .Include(a => a.Author)
                .OrderByDescending(a => a.ViewCount)
                .ToListAsync();
        }

        public async Task<IEnumerable<Articles>> GetMostViewedArticlesAsync(int count)
        {
            return await _dbSet
                .Where(a => a.IsPublished && a.IsActive && !a.IsDeleted)
                .Include(a => a.Author)
                .OrderByDescending(a => a.ViewCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<Articles?> IncrementViewCountAsync(int articleId)
        {
            var article = await _dbSet.FindAsync(articleId);
            if (article != null && !article.IsDeleted)
            {
                article.ViewCount++;
                await _context.SaveChangesAsync();
            }
            return article;
        }
    }
}