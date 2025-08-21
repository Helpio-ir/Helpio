using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.Application.DTOs.Core;

namespace Helpio.Ir.Application.DTOs.Ticketing
{
    public class TicketStateDto : BaseDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ColorCode { get; set; } = "#000000";
        public int Order { get; set; }
        public bool IsDefault { get; set; }
        public bool IsFinal { get; set; }
        public int TicketCount { get; set; }
    }

    public class CreateTicketStateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ColorCode { get; set; } = "#000000";
        public int Order { get; set; }
        public bool IsDefault { get; set; }
        public bool IsFinal { get; set; }
    }

    public class UpdateTicketStateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ColorCode { get; set; } = "#000000";
        public int Order { get; set; }
        public bool IsDefault { get; set; }
        public bool IsFinal { get; set; }
    }
}