using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.Application.DTOs.Core;

namespace Helpio.Ir.Application.DTOs.Ticketing
{
    public enum TicketPriorityDto
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Critical = 4
    }

    public class TicketDto : BaseDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public int TicketStateId { get; set; }
        public int TeamId { get; set; }
        public int? SupportAgentId { get; set; }
        public int TicketCategoryId { get; set; }
        public TicketPriorityDto Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string? Resolution { get; set; }
        public decimal EstimatedHours { get; set; }
        public decimal ActualHours { get; set; }
        
        // Navigation DTOs
        public CustomerDto? Customer { get; set; }
        public TicketStateDto? TicketState { get; set; }
        public TeamDto? Team { get; set; }
        public SupportAgentDto? SupportAgent { get; set; }
        public TicketCategoryDto? TicketCategory { get; set; }
        
        // Counts
        public int ResponseCount { get; set; }
        public int NoteCount { get; set; }
        public int AttachmentCount { get; set; }
        
        // Status
        public bool IsOverdue => DueDate.HasValue && DueDate < DateTime.UtcNow && ResolvedDate == null;
        public bool IsResolved => ResolvedDate.HasValue;
        public TimeSpan? TimeToResolve => ResolvedDate.HasValue ? ResolvedDate - CreatedAt : null;
    }

    public class CreateTicketDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public int TeamId { get; set; }
        public int TicketCategoryId { get; set; }
        public TicketPriorityDto Priority { get; set; } = TicketPriorityDto.Normal;
        public DateTime? DueDate { get; set; }
        public decimal EstimatedHours { get; set; }
    }

    public class UpdateTicketDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? SupportAgentId { get; set; }
        public TicketPriorityDto Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal EstimatedHours { get; set; }
    }

    public class AssignTicketDto
    {
        public int TicketId { get; set; }
        public int SupportAgentId { get; set; }
    }

    public class ResolveTicketDto
    {
        public int TicketId { get; set; }
        public string Resolution { get; set; } = string.Empty;
        public decimal ActualHours { get; set; }
    }
}