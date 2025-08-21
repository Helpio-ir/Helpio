using System;
using System.Collections.Generic;

namespace Helpio.Ir.Domain.Entities.Ticketing
{
    public class Note : BaseEntity
    {
        public int TicketId { get; set; }
        public int? SupportAgentId { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsSystemNote { get; set; } = false;
        public bool IsPrivate { get; set; } = false;
        
        // Navigation properties
        public virtual Ticket Ticket { get; set; } = null!;
        public virtual Core.SupportAgent? SupportAgent { get; set; }
        public virtual ICollection<AttachmentNote> AttachmentNotes { get; set; } = new List<AttachmentNote>();
    }
}