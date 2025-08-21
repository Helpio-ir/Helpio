using Helpio.Ir.Application.DTOs.Common;

namespace Helpio.Ir.Application.DTOs.Core
{
    public class BranchDto : BaseDto
    {
        public int OrganizationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public int? BranchManagerId { get; set; }
        public bool IsActive { get; set; }
        
        // Navigation DTOs
        public OrganizationDto? Organization { get; set; }
        public UserDto? BranchManager { get; set; }
        public int TeamCount { get; set; }
    }

    public class CreateBranchDto
    {
        public int OrganizationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public int? BranchManagerId { get; set; }
    }

    public class UpdateBranchDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public int? BranchManagerId { get; set; }
        public bool IsActive { get; set; }
    }
}