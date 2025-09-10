using System;

namespace Helpio.Ir.Domain.Entities.Core
{
    public class ApiKey : BaseEntity
    {
        public int OrganizationId { get; set; }
        public string KeyName { get; set; } = string.Empty;
        public string KeyValue { get; set; } = string.Empty;
        public string KeyHash { get; set; } = string.Empty; // Hashed version for security
        public bool IsActive { get; set; } = true;
        public DateTime? ExpiresAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public string? AllowedIPs { get; set; } // Comma-separated IPs
        public string? Permissions { get; set; } // JSON string of permissions
        public string? Description { get; set; }
        
        // Navigation properties
        public virtual Organization Organization { get; set; } = null!;
        
        // Computed properties
        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
        public bool IsValid => IsActive && !IsDeleted && !IsExpired;
    }
}