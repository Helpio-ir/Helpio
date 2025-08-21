using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.Application.DTOs.Core;

namespace Helpio.Ir.Application.DTOs.Knowledge
{
    public class CannedResponseDto : BaseDto
    {
        public int OrganizationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Tags { get; set; }
        public bool IsActive { get; set; }
        public int UsageCount { get; set; }
        
        // Navigation DTOs
        public OrganizationDto? Organization { get; set; }
        
        // Computed Properties
        public string[] TagList => Tags?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        public string ShortContent => Content.Length > 100 ? Content[..100] + "..." : Content;
    }

    public class CreateCannedResponseDto
    {
        public int OrganizationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Tags { get; set; }
    }

    public class UpdateCannedResponseDto
    {
        public string Name { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Tags { get; set; }
        public bool IsActive { get; set; }
    }
}