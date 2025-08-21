using System;

namespace Helpio.Ir.Domain.Entities.Knowledge
{
    public class Articles : BaseEntity
    {
        public int OrganizationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Tags { get; set; }
        public bool IsPublished { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public int ViewCount { get; set; } = 0;
        public DateTime? PublishedAt { get; set; }
        public int? AuthorId { get; set; }
        
        // Navigation properties
        public virtual Core.Organization Organization { get; set; } = null!;
        public virtual Core.User? Author { get; set; }
    }
}