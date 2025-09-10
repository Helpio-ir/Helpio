using System;
using System.Collections.Generic;

namespace Helpio.Ir.Domain.Entities.Core
{
    public class Customer : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? CompanyName { get; set; }
        
        // ????? ?? ???? ??? SaaS
        public int? OrganizationId { get; set; }
        
        // Navigation properties
        public virtual Organization? Organization { get; set; }
        public virtual ICollection<Ticketing.Ticket> Tickets { get; set; } = new List<Ticketing.Ticket>();
        public virtual ICollection<Business.Order> Orders { get; set; } = new List<Business.Order>();
    }
}