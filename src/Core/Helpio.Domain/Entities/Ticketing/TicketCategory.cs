using System;
using System.Collections.Generic;

namespace Helpio.Ir.Domain.Entities.Ticketing
{
    public class TicketCategory : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int OrganizationId { get; set; }
        public string ColorCode { get; set; } = "#000000";
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual Core.Organization Organization { get; set; } = null!;
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}