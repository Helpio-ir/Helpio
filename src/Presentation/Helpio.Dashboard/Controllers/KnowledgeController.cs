using AutoMapper;
using Helpio.Dashboard.Services;
using Helpio.Ir.Application.DTOs.Knowledge;
using Helpio.Ir.Domain.Entities.Knowledge;
using Helpio.Ir.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Helpio.Dashboard.Controllers
{
    public class KnowledgeController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public KnowledgeController(ICurrentUserContext userContext, ApplicationDbContext context, IMapper mapper)
            : base(userContext)
        {
            _context = context;
            _mapper = mapper;
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

            // Increment view count
            article.ViewCount++;
            await _context.SaveChangesAsync();

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

                if (article.IsPublished)
                {
                    article.PublishedAt = DateTime.UtcNow;
                }

                _context.Articles.Add(article);
                await _context.SaveChangesAsync();

                TempData["Success"] = "مقاله با موفقیت ایجاد شد.";
                return RedirectToAction(nameof(ArticleDetails), new { id = article.Id });
            }

            return View(article);
        }

        [HttpGet]
        public async Task<IActionResult> EditArticle(int id)
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            var article = await _context.Articles
                .Include(a => a.Author)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article == null || !CanAccessArticle(article))
            {
                return NotFound();
            }

            ViewBag.RelatedArticles = await GetRelatedArticlesAsync(article.Id, article.Tags);
            return View(article);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditArticle(int id, Articles article, string action)
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            if (id != article.Id)
            {
                return BadRequest();
            }

            var existingArticle = await _context.Articles.FindAsync(id);
            if (existingArticle == null || !CanAccessArticle(existingArticle))
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    existingArticle.Name = article.Name;
                    existingArticle.Content = article.Content;
                    existingArticle.Description = article.Description;
                    existingArticle.Tags = article.Tags;
                    existingArticle.IsActive = article.IsActive;
                    existingArticle.UpdatedAt = DateTime.UtcNow;

                    // Handle publish/unpublish
                    if (action == "saveAndPublish" || article.IsPublished)
                    {
                        existingArticle.IsPublished = true;
                        if (!existingArticle.PublishedAt.HasValue)
                        {
                            existingArticle.PublishedAt = DateTime.UtcNow;
                        }
                    }
                    else
                    {
                        existingArticle.IsPublished = article.IsPublished;
                    }

                    await _context.SaveChangesAsync();

                    TempData["Success"] = action == "saveAndPublish" ?
                        "مقاله با موفقیت ذخیره و منتشر شد." : "مقاله با موفقیت بروزرسانی شد.";

                    return RedirectToAction(nameof(ArticleDetails), new { id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ArticleExistsAsync(article.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.RelatedArticles = await GetRelatedArticlesAsync(article.Id, article.Tags);
            return View(article);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            if (!IsCurrentUserAdmin)
            {
                return Forbid("فقط مدیران کل می‌توانند مقالات را حذف کنند.");
            }

            var article = await _context.Articles.FindAsync(id);
            if (article == null || !CanAccessArticle(article))
            {
                return NotFound();
            }

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();

            TempData["Success"] = "مقاله با موفقیت حذف شد.";
            return RedirectToAction(nameof(Articles));
        }

        [HttpGet]
        public IActionResult CreateCannedResponse()
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            var dto = new CreateCannedResponseDto();

            // Set default organization for non-admin users
            if (!IsCurrentUserAdmin && CurrentOrganizationId.HasValue)
            {
                dto.OrganizationId = CurrentOrganizationId.Value;
            }

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCannedResponse(CreateCannedResponseDto dto)
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            // Set OrganizationId before validation
            if (!IsCurrentUserAdmin && CurrentOrganizationId.HasValue)
            {
                dto.OrganizationId = CurrentOrganizationId.Value;
            }
            else if (IsCurrentUserAdmin && dto.OrganizationId == 0)
            {
                // For admin users, if no organization is specified, use the first available organization
                var firstOrganization = await _context.Organizations.FirstOrDefaultAsync();
                if (firstOrganization != null)
                {
                    dto.OrganizationId = firstOrganization.Id;
                }
            }

            // Manual validation for OrganizationId
            if (dto.OrganizationId <= 0)
            {
                ModelState.AddModelError(nameof(dto.OrganizationId), "انتخاب سازمان الزامی است.");
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                ModelState.AddModelError(nameof(dto.Name), "نام پاسخ آماده الزامی است.");
            }

            if (string.IsNullOrWhiteSpace(dto.Content))
            {
                ModelState.AddModelError(nameof(dto.Content), "محتوای پاسخ الزامی است.");
            }

            // Check for duplicate names within the same organization
            var existingResponse = await _context.CannedResponses
                .FirstOrDefaultAsync(cr => cr.Name == dto.Name && cr.OrganizationId == dto.OrganizationId && !cr.IsDeleted);

            if (existingResponse != null)
            {
                ModelState.AddModelError(nameof(dto.Name), "پاسخ آماده با این نام قبلاً در این سازمان ثبت شده است.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Map DTO to Entity
                    var response = _mapper.Map<CannedResponse>(dto);
                    response.CreatedAt = DateTime.UtcNow;
                    response.IsActive = dto.IsActive;
                    response.UsageCount = 0;

                    _context.CannedResponses.Add(response);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "پاسخ آماده با موفقیت ایجاد شد.";
                    return RedirectToAction(nameof(CannedResponses));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"خطا در ایجاد پاسخ آماده: {ex.Message}");
                }
            }

            // If we reach here, there are validation errors
            await SetViewBagDataForCannedResponse();
            return View(dto);
        }

        [HttpGet]
        public async Task<IActionResult> EditCannedResponse(int id)
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            var response = await _context.CannedResponses.FindAsync(id);
            if (response == null || !CanAccessCannedResponse(response))
            {
                return NotFound();
            }

            // Map Entity to Update DTO
            var dto = new UpdateCannedResponseDto
            {
                Name = response.Name,
                Content = response.Content,
                Description = response.Description,
                Tags = response.Tags,
                IsActive = response.IsActive
            };

            ViewBag.Id = response.Id;
            ViewBag.OrganizationId = response.OrganizationId;
            ViewBag.UsageCount = response.UsageCount;
            ViewBag.CreatedAt = response.CreatedAt;
            ViewBag.UpdatedAt = response.UpdatedAt;

            await SetViewBagDataForCannedResponse();
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCannedResponse(int id, UpdateCannedResponseDto dto)
        {
            if (!IsCurrentUserAdmin && !IsCurrentUserManager)
            {
                return Forbid();
            }

            var existingResponse = await _context.CannedResponses.FindAsync(id);
            if (existingResponse == null || !CanAccessCannedResponse(existingResponse))
            {
                return NotFound();
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                ModelState.AddModelError(nameof(dto.Name), "نام پاسخ آماده الزامی است.");
            }

            if (string.IsNullOrWhiteSpace(dto.Content))
            {
                ModelState.AddModelError(nameof(dto.Content), "محتوای پاسخ الزامی است.");
            }

            // Check for duplicate names within the same organization (excluding current response)
            var duplicateResponse = await _context.CannedResponses
                .FirstOrDefaultAsync(cr => cr.Name == dto.Name &&
                                          cr.OrganizationId == existingResponse.OrganizationId &&
                                          cr.Id != id &&
                                          !cr.IsDeleted);

            if (duplicateResponse != null)
            {
                ModelState.AddModelError(nameof(dto.Name), "پاسخ آماده با این نام قبلاً در این سازمان ثبت شده است.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update existing entity with DTO data
                    existingResponse.Name = dto.Name;
                    existingResponse.Content = dto.Content;
                    existingResponse.Description = dto.Description;
                    existingResponse.Tags = dto.Tags;
                    existingResponse.IsActive = dto.IsActive;
                    existingResponse.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "پاسخ آماده با موفقیت بروزرسانی شد.";
                    return RedirectToAction(nameof(CannedResponses));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await CannedResponseExistsAsync(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"خطا در بروزرسانی پاسخ آماده: {ex.Message}");
                }
            }

            // If we reach here, there are validation errors
            ViewBag.Id = existingResponse.Id;
            ViewBag.OrganizationId = existingResponse.OrganizationId;
            ViewBag.UsageCount = existingResponse.UsageCount;
            ViewBag.CreatedAt = existingResponse.CreatedAt;
            ViewBag.UpdatedAt = existingResponse.UpdatedAt;

            await SetViewBagDataForCannedResponse();
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCannedResponse(int id)
        {
            if (!IsCurrentUserAdmin)
            {
                return Forbid("فقط مدیران کل می‌توانند پاسخ‌های آماده را حذف کنند.");
            }

            var response = await _context.CannedResponses.FindAsync(id);
            if (response == null || !CanAccessCannedResponse(response))
            {
                return NotFound();
            }

            _context.CannedResponses.Remove(response);
            await _context.SaveChangesAsync();

            TempData["Success"] = "پاسخ آماده با موفقیت حذف شد.";
            return RedirectToAction(nameof(CannedResponses));
        }

        [HttpGet]
        public async Task<IActionResult> GetCannedResponses()
        {
            try
            {
                var responses = await GetAccessibleCannedResponsesAsync();

                var result = responses.Where(r => r.IsActive).Select(r => new
                {
                    id = r.Id,
                    name = r.Name,
                    content = r.Content,
                    description = r.Description,
                    tags = r.Tags,
                    usageCount = r.UsageCount,
                    isActive = r.IsActive
                }).ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { error = $"خطا در بارگذاری پاسخ‌های آماده: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> IncrementCannedResponseUsage(int id)
        {
            try
            {
                var response = await _context.CannedResponses.FindAsync(id);
                if (response != null && CanAccessCannedResponse(response))
                {
                    response.UsageCount++;
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, newUsageCount = response.UsageCount });
                }
                return Json(new { success = false, message = "پاسخ آماده یافت نشد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
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

        private async Task<List<Articles>> GetRelatedArticlesAsync(int currentArticleId, string? tags)
        {
            if (string.IsNullOrEmpty(tags))
                return new List<Articles>();

            var tagList = tags.Split(',').Select(t => t.Trim()).ToList();

            return await _context.Articles
                .Where(a => a.Id != currentArticleId &&
                           a.IsPublished &&
                           a.IsActive &&
                           !a.IsDeleted &&
                           a.Tags != null &&
                           tagList.Any(tag => a.Tags.Contains(tag)))
                .Take(5)
                .ToListAsync();
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

        private bool CanAccessCannedResponse(CannedResponse response)
        {
            if (IsCurrentUserAdmin) return true;

            if (CurrentOrganizationId.HasValue)
            {
                return response.OrganizationId == CurrentOrganizationId.Value;
            }

            return false;
        }

        private async Task<bool> ArticleExistsAsync(int id)
        {
            return await _context.Articles.AnyAsync(e => e.Id == id);
        }

        private async Task<bool> CannedResponseExistsAsync(int id)
        {
            return await _context.CannedResponses.AnyAsync(e => e.Id == id);
        }

        private async Task SetViewBagDataForCannedResponse()
        {
            // Set available organizations for admin users
            if (IsCurrentUserAdmin)
            {
                ViewBag.Organizations = await _context.Organizations
                    .Where(o => o.IsActive)
                    .OrderBy(o => o.Name)
                    .ToListAsync();
            }
            else if (CurrentOrganizationId.HasValue)
            {
                ViewBag.Organizations = await _context.Organizations
                    .Where(o => o.Id == CurrentOrganizationId.Value)
                    .ToListAsync();
            }
        }
    }
}