using Helpio.Dashboard.Services;
using Helpio.Ir.Domain.Entities.Knowledge;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Dashboard.Controllers
{
    public class KnowledgeController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public KnowledgeController(ICurrentUserContext userContext, ApplicationDbContext context)
            : base(userContext)
        {
            _context = context;
        }

        public async Task<IActionResult> Articles()
        {
            var articles = await GetAccessibleArticlesAsync();
            return View(articles);
        }

        public async Task<IActionResult> CannedResponses()
        {
            var responses = await GetAccessibleCannedResponsesAsync();
            return View(responses);
        }

        public async Task<IActionResult> ArticleDetails(int id)
        {
            var article = await _context.Articles
                .Include(a => a.Organization)
                .Include(a => a.Author)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null || !CanAccessArticle(article))
            {
                return NotFound();
            }

            return View(article);
        }

        [HttpGet]
        public IActionResult CreateArticle()
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            return View(new Articles());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateArticle(Articles article)
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                article.CreatedAt = DateTime.UtcNow;
                article.AuthorId = UserContext.UserId;

                if (!IsCurrentUserAdmin && CurrentOrganizationId.HasValue)
                {
                    article.OrganizationId = CurrentOrganizationId.Value;
                }

                _context.Articles.Add(article);
                await _context.SaveChangesAsync();

                TempData["Success"] = "مقاله با موفقیت ایجاد شد.";
                return RedirectToAction(nameof(ArticleDetails), new { id = article.Id });
            }

            return View(article);
        }

        [HttpGet]
        public IActionResult CreateCannedResponse()
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            return View(new CannedResponse());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCannedResponse(CannedResponse response)
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                response.CreatedAt = DateTime.UtcNow;

                if (!IsCurrentUserAdmin && CurrentOrganizationId.HasValue)
                {
                    response.OrganizationId = CurrentOrganizationId.Value;
                }

                _context.CannedResponses.Add(response);
                await _context.SaveChangesAsync();

                TempData["Success"] = "پاسخ آماده با موفقیت ایجاد شد.";
                return RedirectToAction(nameof(CannedResponses));
            }

            return View(response);
        }

        private async Task<List<Articles>> GetAccessibleArticlesAsync()
        {
            var query = _context.Articles
                .Include(a => a.Organization)
                .Include(a => a.Author)
                .Where(a => !a.IsDeleted)
                .AsQueryable();

            if (!IsCurrentUserAdmin && CurrentOrganizationId.HasValue)
            {
                query = query.Where(a => a.OrganizationId == CurrentOrganizationId.Value);
            }

            return await query.OrderByDescending(a => a.CreatedAt).ToListAsync();
        }

        private async Task<List<CannedResponse>> GetAccessibleCannedResponsesAsync()
        {
            var query = _context.CannedResponses
                .Include(cr => cr.Organization)
                .Where(cr => !cr.IsDeleted)
                .AsQueryable();

            if (!IsCurrentUserAdmin && CurrentOrganizationId.HasValue)
            {
                query = query.Where(cr => cr.OrganizationId == CurrentOrganizationId.Value);
            }

            return await query.OrderBy(cr => cr.Name).ToListAsync();
        }

        private bool CanAccessArticle(Articles article)
        {
            if (IsCurrentUserAdmin) return true;

            if (CurrentOrganizationId.HasValue)
            {
                return article.OrganizationId == CurrentOrganizationId.Value;
            }

            return false;
        }
    }
}