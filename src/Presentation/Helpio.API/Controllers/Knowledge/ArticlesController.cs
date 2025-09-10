using Microsoft.AspNetCore.Mvc;
using Helpio.Ir.Application.Services.Knowledge;
using Helpio.Ir.Application.DTOs.Knowledge;
using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.API.Services;
using FluentValidation;

namespace Helpio.Ir.API.Controllers.Knowledge
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArticlesController : ControllerBase
    {
        private readonly IArticlesService _articlesService;
        private readonly IOrganizationContext _organizationContext;
        private readonly IValidator<CreateArticlesDto> _createValidator;
        private readonly IValidator<UpdateArticlesDto> _updateValidator;
        private readonly ILogger<ArticlesController> _logger;

        public ArticlesController(
            IArticlesService articlesService,
            IOrganizationContext organizationContext,
            IValidator<CreateArticlesDto> createValidator,
            IValidator<UpdateArticlesDto> updateValidator,
            ILogger<ArticlesController> logger)
        {
            _articlesService = articlesService;
            _organizationContext = organizationContext;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        /// <summary>
        /// Get all articles with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<ArticlesDto>>> GetArticles([FromQuery] PaginationRequest request)
        {
            try
            {
                var result = await _articlesService.GetArticlesAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving articles");
                return BadRequest("Error retrieving articles");
            }
        }

        /// <summary>
        /// Get article by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ArticlesDto>> GetArticle(int id)
        {
            var article = await _articlesService.GetByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            // ????? ?????? ??????? - ??? ??? ????? ????? ???? ??? ????
            if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
            {
                if (article.OrganizationId != _organizationContext.OrganizationId.Value)
                {
                    // ??? ????? ????? ??? ????? ???? ?????? ???
                    if (!article.IsPublished)
                    {
                        return Forbid("Access denied to other organization's unpublished articles");
                    }
                }
            }
            // ??? ????? ????? ???? ????? ??? ?????? ????? ??? ???? ?????? ???
            else if (!article.IsPublished)
            {
                return NotFound(); // ???? ??????? ????? ???? ????? draft ?? ???? ?????? ??????
            }

            return Ok(article);
        }

        /// <summary>
        /// Get article by ID and increment view count
        /// </summary>
        [HttpGet("{id}/view")]
        public async Task<ActionResult<ArticlesDto>> ViewArticle(int id)
        {
            try
            {
                // ????? ????? ??????? ?? ??? ????? ?? ?????? ????
                var article = await _articlesService.GetByIdAsync(id);
                if (article == null)
                {
                    return NotFound();
                }

                // ????? ?????? ???????
                if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
                {
                    if (article.OrganizationId != _organizationContext.OrganizationId.Value)
                    {
                        if (!article.IsPublished)
                        {
                            return Forbid("Access denied to other organization's unpublished articles");
                        }
                    }
                }
                else if (!article.IsPublished)
                {
                    return NotFound();
                }

                // ???? view count ?? ?????? ???????
                var updatedArticle = await _articlesService.IncrementViewCountAsync(id);
                return Ok(updatedArticle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error viewing article: {ArticleId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Create a new article
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ArticlesDto>> CreateArticle(CreateArticlesDto createDto)
        {
            // Validate input
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            // Ensure article is created for the authenticated organization
            if (_organizationContext.OrganizationId.HasValue)
            {
                createDto.OrganizationId = _organizationContext.OrganizationId.Value;
            }

            try
            {
                var result = await _articlesService.CreateAsync(createDto);
                
                _logger.LogInformation("Article created: {ArticleName} for Organization: {OrganizationId}", 
                    result.Name, createDto.OrganizationId);

                return CreatedAtAction(nameof(GetArticle), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating article: {ArticleName}", createDto.Name);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update article
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ArticlesDto>> UpdateArticle(int id, UpdateArticlesDto updateDto)
        {
            // ????? ????? ????
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            if (!_organizationContext.OrganizationId.HasValue)
            {
                return BadRequest("Organization context not found");
            }

            // ????? ???? ????? ? ?????? ???????
            var existingArticle = await _articlesService.GetByIdAsync(id);
            if (existingArticle == null)
            {
                return NotFound();
            }

            if (existingArticle.OrganizationId != _organizationContext.OrganizationId.Value)
            {
                return Forbid("Access denied to other organization's articles");
            }

            // Validate input
            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _articlesService.UpdateAsync(id, updateDto);
                
                _logger.LogInformation("Article updated: {ArticleId}", id);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating article: {ArticleId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete article
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArticle(int id)
        {
            // ????? ????? ????
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            if (!_organizationContext.OrganizationId.HasValue)
            {
                return BadRequest("Organization context not found");
            }

            // ????? ???? ????? ? ?????? ???????
            var existingArticle = await _articlesService.GetByIdAsync(id);
            if (existingArticle == null)
            {
                return NotFound();
            }

            if (existingArticle.OrganizationId != _organizationContext.OrganizationId.Value)
            {
                return Forbid("Access denied to other organization's articles");
            }

            try
            {
                var result = await _articlesService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                _logger.LogInformation("Article deleted: {ArticleId}", id);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting article: {ArticleId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get articles by organization ID
        /// </summary>
        [HttpGet("organization/{organizationId}")]
        public async Task<ActionResult<IEnumerable<ArticlesDto>>> GetArticlesByOrganization(int organizationId)
        {
            // ??? ????? ??????? ?? ????? ????? ???? ??? ????
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            // ??? ????? ??????? ?? OrganizationId ????? ????
            if (!_organizationContext.OrganizationId.HasValue)
            {
                return BadRequest("Organization context not found");
            }

            // ?? ????? ????? ??????? ?? ????? ??? ?? ???????? ?????? ???? ?????? ????? ????
            if (_organizationContext.OrganizationId.Value != organizationId)
            {
                return Forbid("Access denied to other organization's articles");
            }

            try
            {
                var articles = await _articlesService.GetByOrganizationIdAsync(organizationId);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving articles for organization: {OrganizationId}", organizationId);
                return BadRequest("Error retrieving articles");
            }
        }

        /// <summary>
        /// Get articles for authenticated organization
        /// </summary>
        [HttpGet("my-organization")]
        public async Task<ActionResult<IEnumerable<ArticlesDto>>> GetMyOrganizationArticles()
        {
            if (!_organizationContext.OrganizationId.HasValue)
            {
                return BadRequest("Organization context not found");
            }

            try
            {
                var articles = await _articlesService.GetByOrganizationIdAsync(_organizationContext.OrganizationId.Value);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving articles for organization: {OrganizationId}", _organizationContext.OrganizationId);
                return BadRequest("Error retrieving articles");
            }
        }

        /// <summary>
        /// Get published articles
        /// </summary>
        [HttpGet("published")]
        public async Task<ActionResult<IEnumerable<ArticlesDto>>> GetPublishedArticles()
        {
            try
            {
                var articles = await _articlesService.GetPublishedArticlesAsync();
                return Ok(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving published articles");
                return BadRequest("Error retrieving published articles");
            }
        }

        /// <summary>
        /// Get articles by author ID
        /// </summary>
        [HttpGet("author/{authorId}")]
        public async Task<ActionResult<IEnumerable<ArticlesDto>>> GetArticlesByAuthor(int authorId)
        {
            try
            {
                var articles = await _articlesService.GetByAuthorIdAsync(authorId);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving articles for author: {AuthorId}", authorId);
                return BadRequest("Error retrieving articles");
            }
        }

        /// <summary>
        /// Search articles by tags
        /// </summary>
        [HttpGet("search/tags")]
        public async Task<ActionResult<IEnumerable<ArticlesDto>>> SearchArticlesByTags([FromQuery] string tags)
        {
            if (string.IsNullOrWhiteSpace(tags))
            {
                return BadRequest("Tags parameter is required");
            }

            try
            {
                var articles = await _articlesService.SearchByTagsAsync(tags);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching articles by tags: {Tags}", tags);
                return BadRequest("Error searching articles");
            }
        }

        /// <summary>
        /// Get most viewed articles
        /// </summary>
        [HttpGet("most-viewed")]
        public async Task<ActionResult<IEnumerable<ArticlesDto>>> GetMostViewedArticles([FromQuery] int count = 10)
        {
            if (count <= 0 || count > 100)
            {
                return BadRequest("Count must be between 1 and 100");
            }

            try
            {
                var articles = await _articlesService.GetMostViewedArticlesAsync(count);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving most viewed articles");
                return BadRequest("Error retrieving most viewed articles");
            }
        }

        /// <summary>
        /// Publish an article
        /// </summary>
        [HttpPost("{id}/publish")]
        public async Task<IActionResult> PublishArticle(int id)
        {
            // ????? ????? ????
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            if (!_organizationContext.OrganizationId.HasValue)
            {
                return BadRequest("Organization context not found");
            }

            // ????? ???? ????? ? ?????? ???????
            var existingArticle = await _articlesService.GetByIdAsync(id);
            if (existingArticle == null)
            {
                return NotFound();
            }

            if (existingArticle.OrganizationId != _organizationContext.OrganizationId.Value)
            {
                return Forbid("Access denied to other organization's articles");
            }

            try
            {
                var result = await _articlesService.PublishArticleAsync(id);
                if (!result)
                {
                    return BadRequest("Article is already published or not found");
                }

                _logger.LogInformation("Article {ArticleId} published", id);
                
                return Ok(new { message = "Article published successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing article: {ArticleId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Unpublish an article
        /// </summary>
        [HttpPost("{id}/unpublish")]
        public async Task<IActionResult> UnpublishArticle(int id)
        {
            // ????? ????? ????
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            if (!_organizationContext.OrganizationId.HasValue)
            {
                return BadRequest("Organization context not found");
            }

            // ????? ???? ????? ? ?????? ???????
            var existingArticle = await _articlesService.GetByIdAsync(id);
            if (existingArticle == null)
            {
                return NotFound();
            }

            if (existingArticle.OrganizationId != _organizationContext.OrganizationId.Value)
            {
                return Forbid("Access denied to other organization's articles");
            }

            try
            {
                var result = await _articlesService.UnpublishArticleAsync(id);
                if (!result)
                {
                    return BadRequest("Article is already unpublished or not found");
                }

                _logger.LogInformation("Article {ArticleId} unpublished", id);
                
                return Ok(new { message = "Article unpublished successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unpublishing article: {ArticleId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get article statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetArticleStatistics()
        {
            try
            {
                // Get articles for the organization if context is available
                IEnumerable<ArticlesDto> articles;
                if (_organizationContext.OrganizationId.HasValue)
                {
                    articles = await _articlesService.GetByOrganizationIdAsync(_organizationContext.OrganizationId.Value);
                }
                else
                {
                    // Get all published articles if no organization context
                    articles = await _articlesService.GetPublishedArticlesAsync();
                }

                var statistics = new
                {
                    TotalArticles = articles.Count(),
                    PublishedArticles = articles.Count(a => a.IsPublished),
                    DraftArticles = articles.Count(a => !a.IsPublished),
                    TotalViews = articles.Sum(a => a.ViewCount),
                    AverageViews = articles.Any() ? articles.Average(a => a.ViewCount) : 0,
                    MostViewedArticle = articles.OrderByDescending(a => a.ViewCount).FirstOrDefault(),
                    RecentlyPublished = articles.Count(a => a.IsRecentlyPublished)
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving article statistics");
                return BadRequest("Error retrieving statistics");
            }
        }
    }
}