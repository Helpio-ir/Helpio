using System;
using System.Collections.Generic;

namespace Helpio.Ir.Domain.Entities.Core
{
    public class Organization : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
        public virtual ICollection<Ticketing.TicketCategory> TicketCategories { get; set; } = new List<Ticketing.TicketCategory>();
        public virtual ICollection<Knowledge.CannedResponse> CannedResponses { get; set; } = new List<Knowledge.CannedResponse>();
        public virtual ICollection<Knowledge.Articles> Articles { get; set; } = new List<Knowledge.Articles>();
    }
}