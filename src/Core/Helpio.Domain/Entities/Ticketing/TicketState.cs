using System;
using System.Collections.Generic;

namespace Helpio.Ir.Domain.Entities.Ticketing
{
    public class TicketState : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string ColorCode { get; set; } = "#000000";
        public int Order { get; set; }
        public bool IsDefault { get; set; } = false;
        public bool IsFinal { get; set; } = false;
        
        // Navigation properties
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}