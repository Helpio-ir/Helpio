using System;
using System.Collections.Generic;

namespace Helpio.Ir.Domain.Entities.Ticketing
{
    public enum TicketPriority
    {
        Low = 1,
        Normal = 2,
        High = 3,
        Critical = 4
    }

    public class Ticket : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public int TicketStateId { get; set; }
        public int TeamId { get; set; }
        public int? SupportAgentId { get; set; }
        public int TicketCategoryId { get; set; }
        public TicketPriority Priority { get; set; } = TicketPriority.Normal;
        public DateTime? DueDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string? Resolution { get; set; }
        public decimal EstimatedHours { get; set; }
        public decimal ActualHours { get; set; }
        
        // Navigation properties
        public virtual Core.Customer Customer { get; set; } = null!;
        public virtual TicketState TicketState { get; set; } = null!;
        public virtual Core.Team Team { get; set; } = null!;
        public virtual Core.SupportAgent? SupportAgent { get; set; }
        public virtual TicketCategory TicketCategory { get; set; } = null!;
        public virtual ICollection<Response> Responses { get; set; } = new List<Response>();
        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
        public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}