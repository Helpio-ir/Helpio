using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.Application.DTOs.Core;

namespace Helpio.Ir.Application.DTOs.Knowledge
{
    public class ArticlesDto : BaseDto
    {
        public int OrganizationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Tags { get; set; }
        public bool IsPublished { get; set; }
        public bool IsActive { get; set; }
        public int ViewCount { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int? AuthorId { get; set; }
        
        // Navigation DTOs
        public OrganizationDto? Organization { get; set; }
        public UserDto? Author { get; set; }
        
        // Computed Properties
        public string[] TagList => Tags?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        public string ShortContent => Content.Length > 200 ? Content[..200] + "..." : Content;
        public string AuthorName => Author?.FullName ?? "??????";
        public bool IsRecentlyPublished => PublishedAt.HasValue && PublishedAt.Value >= DateTime.UtcNow.AddDays(-7);
    }

    public class CreateArticlesDto
    {
        public int OrganizationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Tags { get; set; }
        public bool IsPublished { get; set; }
        public int? AuthorId { get; set; }
    }

    public class UpdateArticlesDto
    {
        public string Name { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Tags { get; set; }
        public bool IsPublished { get; set; }
        public bool IsActive { get; set; }
    }
}