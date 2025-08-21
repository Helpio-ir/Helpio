using AutoMapper;
using Microsoft.Extensions.Logging;
using Helpio.Ir.Application.DTOs.Knowledge;
using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.Application.Services.Knowledge;
using Helpio.Ir.Application.Common.Exceptions;
using Helpio.Ir.Application.Common.Interfaces;
using Helpio.Ir.Domain.Interfaces;
using Helpio.Ir.Domain.Entities.Knowledge;

namespace Helpio.Ir.Application.Services.Knowledge
{
    public class ArticlesService : IArticlesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ArticlesService> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDateTime _dateTime;

        public ArticlesService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ArticlesService> logger,
            ICurrentUserService currentUserService,
            IDateTime dateTime)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
            _dateTime = dateTime;
        }

        public async Task<ArticlesDto?> GetByIdAsync(int id)
        {
            var article = await _unitOfWork.Articles.GetByIdAsync(id);
            return article != null ? _mapper.Map<ArticlesDto>(article) : null;
        }

        public async Task<PaginatedResult<ArticlesDto>> GetArticlesAsync(PaginationRequest request)
        {
            var articles = await _unitOfWork.Articles.GetAllAsync();

            // Apply search filter
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                articles = articles.Where(a =>
                    a.Name.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    a.Content.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (a.Description != null && a.Description.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (a.Tags != null && a.Tags.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            // Apply sorting
            articles = request.SortBy?.ToLower() switch
            {
                "name" => request.SortDescending ? articles.OrderByDescending(a => a.Name) : articles.OrderBy(a => a.Name),
                "viewcount" => request.SortDescending ? articles.OrderByDescending(a => a.ViewCount) : articles.OrderBy(a => a.ViewCount),
                "publishedat" => request.SortDescending ? articles.OrderByDescending(a => a.PublishedAt) : articles.OrderBy(a => a.PublishedAt),
                "createdat" => request.SortDescending ? articles.OrderByDescending(a => a.CreatedAt) : articles.OrderBy(a => a.CreatedAt),
                _ => articles.OrderByDescending(a => a.CreatedAt)
            };

            var totalItems = articles.Count();
            var items = articles
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var articleDtos = _mapper.Map<List<ArticlesDto>>(items);

            return new PaginatedResult<ArticlesDto>
            {
                Items = articleDtos,
                TotalItems = totalItems,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<ArticlesDto> CreateAsync(CreateArticlesDto createDto)
        {
            // Validate organization exists
            var organization = await _unitOfWork.Organizations.GetByIdAsync(createDto.OrganizationId);
            if (organization == null)
            {
                throw new NotFoundException("Organization", createDto.OrganizationId);
            }

            // Validate author exists if provided
            if (createDto.AuthorId.HasValue)
            {
                var author = await _unitOfWork.Users.GetByIdAsync(createDto.AuthorId.Value);
                if (author == null)
                {
                    throw new NotFoundException("User", createDto.AuthorId.Value);
                }
            }

            var article = _mapper.Map<Articles>(createDto);
            
            // Set author from current user if not provided
            if (!article.AuthorId.HasValue && _currentUserService.UserId.HasValue)
            {
                article.AuthorId = _currentUserService.UserId.Value;
            }

            var createdArticle = await _unitOfWork.Articles.AddAsync(article);

            _logger.LogInformation("Article created with ID: {ArticleId}, Name: {Name}", 
                createdArticle.Id, createdArticle.Name);

            return _mapper.Map<ArticlesDto>(createdArticle);
        }

        public async Task<ArticlesDto> UpdateAsync(int id, UpdateArticlesDto updateDto)
        {
            var article = await _unitOfWork.Articles.GetByIdAsync(id);
            if (article == null)
            {
                throw new NotFoundException("Articles", id);
            }

            var wasPublished = article.IsPublished;
            _mapper.Map(updateDto, article);

            // Set published date if article is being published for the first time
            if (!wasPublished && updateDto.IsPublished)
            {
                article.PublishedAt = _dateTime.UtcNow;
            }
            // Clear published date if article is being unpublished
            else if (wasPublished && !updateDto.IsPublished)
            {
                article.PublishedAt = null;
            }

            await _unitOfWork.Articles.UpdateAsync(article);

            _logger.LogInformation("Article updated with ID: {ArticleId}", id);

            return _mapper.Map<ArticlesDto>(article);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var article = await _unitOfWork.Articles.GetByIdAsync(id);
            if (article == null)
            {
                return false;
            }

            await _unitOfWork.Articles.DeleteAsync(article);

            _logger.LogInformation("Article deleted with ID: {ArticleId}", id);

            return true;
        }

        public async Task<IEnumerable<ArticlesDto>> GetByOrganizationIdAsync(int organizationId)
        {
            var articles = await _unitOfWork.Articles.GetByOrganizationIdAsync(organizationId);
            return _mapper.Map<IEnumerable<ArticlesDto>>(articles);
        }

        public async Task<IEnumerable<ArticlesDto>> GetPublishedArticlesAsync()
        {
            var articles = await _unitOfWork.Articles.GetPublishedArticlesAsync();
            return _mapper.Map<IEnumerable<ArticlesDto>>(articles);
        }

        public async Task<IEnumerable<ArticlesDto>> GetByAuthorIdAsync(int authorId)
        {
            var articles = await _unitOfWork.Articles.GetByAuthorIdAsync(authorId);
            return _mapper.Map<IEnumerable<ArticlesDto>>(articles);
        }

        public async Task<IEnumerable<ArticlesDto>> SearchByTagsAsync(string tags)
        {
            var articles = await _unitOfWork.Articles.SearchByTagsAsync(tags);
            return _mapper.Map<IEnumerable<ArticlesDto>>(articles);
        }

        public async Task<IEnumerable<ArticlesDto>> GetMostViewedArticlesAsync(int count)
        {
            var articles = await _unitOfWork.Articles.GetMostViewedArticlesAsync(count);
            return _mapper.Map<IEnumerable<ArticlesDto>>(articles);
        }

        public async Task<ArticlesDto?> IncrementViewCountAsync(int articleId)
        {
            var article = await _unitOfWork.Articles.IncrementViewCountAsync(articleId);
            if (article == null)
            {
                throw new NotFoundException("Articles", articleId);
            }

            _logger.LogInformation("Article {ArticleId} view count incremented to {ViewCount}", 
                articleId, article.ViewCount);

            return _mapper.Map<ArticlesDto>(article);
        }

        public async Task<bool> PublishArticleAsync(int articleId)
        {
            var article = await _unitOfWork.Articles.GetByIdAsync(articleId);
            if (article == null)
            {
                throw new NotFoundException("Articles", articleId);
            }

            if (article.IsPublished)
            {
                return false; // Already published
            }

            article.IsPublished = true;
            article.PublishedAt = _dateTime.UtcNow;
            await _unitOfWork.Articles.UpdateAsync(article);

            _logger.LogInformation("Article {ArticleId} published", articleId);

            return true;
        }

        public async Task<bool> UnpublishArticleAsync(int articleId)
        {
            var article = await _unitOfWork.Articles.GetByIdAsync(articleId);
            if (article == null)
            {
                throw new NotFoundException("Articles", articleId);
            }

            if (!article.IsPublished)
            {
                return false; // Already unpublished
            }

            article.IsPublished = false;
            article.PublishedAt = null;
            await _unitOfWork.Articles.UpdateAsync(article);

            _logger.LogInformation("Article {ArticleId} unpublished", articleId);

            return true;
        }
    }
}