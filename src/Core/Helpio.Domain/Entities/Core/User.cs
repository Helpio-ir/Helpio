using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Helpio.Ir.Domain.Entities.Core
{
    public class User : IdentityUser<int>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation properties
        public virtual ICollection<Ticketing.Ticket> AssignedTickets { get; set; } = new List<Ticketing.Ticket>();
        public virtual ICollection<Ticketing.Response> Responses { get; set; } = new List<Ticketing.Response>();
        public virtual ICollection<Ticketing.Note> Notes { get; set; } = new List<Ticketing.Note>();

        // Full name property for convenience
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}