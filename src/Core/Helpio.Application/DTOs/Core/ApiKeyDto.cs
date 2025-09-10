using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.Application.DTOs.Core;

namespace Helpio.Ir.Application.DTOs.Core
{
    public class ApiKeyDto : BaseDto
    {
        public int OrganizationId { get; set; }
        public string KeyName { get; set; } = string.Empty;
        public string KeyValue { get; set; } = string.Empty; // Will be masked in responses
        public bool IsActive { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
        public string? AllowedIPs { get; set; }
        public string? Permissions { get; set; }
        public string? Description { get; set; }
        
        // Navigation DTOs
        public OrganizationDto? Organization { get; set; }
        
        // Computed Properties
        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
        public bool IsValid => IsActive && !IsExpired;
        public string MaskedKey => KeyValue.Length > 8 ? $"{KeyValue[..4]}****{KeyValue[^4..]}" : "****";
        public string[] AllowedIPList => AllowedIPs?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        public int DaysUntilExpiry => ExpiresAt.HasValue ? Math.Max(0, (int)(ExpiresAt.Value - DateTime.UtcNow).TotalDays) : -1;
    }

    public class CreateApiKeyDto
    {
        public int OrganizationId { get; set; }
        public string KeyName { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
        public string? AllowedIPs { get; set; }
        public string? Permissions { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateApiKeyDto
    {
        public string KeyName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? AllowedIPs { get; set; }
        public string? Permissions { get; set; }
        public string? Description { get; set; }
    }

    public class ApiKeyResponseDto
    {
        public int Id { get; set; }
        public string KeyName { get; set; } = string.Empty;
        public string KeyValue { get; set; } = string.Empty; // Only returned once during creation
        public string Message { get; set; } = string.Empty;
    }
}