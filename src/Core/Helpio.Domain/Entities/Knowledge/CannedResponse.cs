using System;

namespace Helpio.Ir.Domain.Entities.Knowledge
{
    public class CannedResponse : BaseEntity
    {
        public int OrganizationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Tags { get; set; }
        public bool IsActive { get; set; } = true;
        public int UsageCount { get; set; } = 0;
        
        // Navigation properties
        public virtual Core.Organization Organization { get; set; } = null!;
    }
}