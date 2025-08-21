using System;
using System.Collections.Generic;

namespace Helpio.Ir.Domain.Entities.Ticketing
{
    public class Response : BaseEntity
    {
        public int TicketId { get; set; }
        public int? UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsFromCustomer { get; set; } = false;
        public bool IsPrivate { get; set; } = false;
        public DateTime? ReadAt { get; set; }
        
        // Navigation properties
        public virtual Ticket Ticket { get; set; } = null!;
        public virtual Core.User? User { get; set; }
        public virtual ICollection<AttachmentResponse> AttachmentResponses { get; set; } = new List<AttachmentResponse>();
    }
}