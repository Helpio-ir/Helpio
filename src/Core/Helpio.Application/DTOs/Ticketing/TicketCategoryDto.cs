using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.Application.DTOs.Core;

namespace Helpio.Ir.Application.DTOs.Ticketing
{
    public class TicketCategoryDto : BaseDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int OrganizationId { get; set; }
        public string ColorCode { get; set; } = "#000000";
        public bool IsActive { get; set; }
        
        // Navigation DTOs
        public OrganizationDto? Organization { get; set; }
        public int TicketCount { get; set; }
    }

    public class CreateTicketCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int OrganizationId { get; set; }
        public string ColorCode { get; set; } = "#000000";
    }

    public class UpdateTicketCategoryDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ColorCode { get; set; } = "#000000";
        public bool IsActive { get; set; }
    }
}