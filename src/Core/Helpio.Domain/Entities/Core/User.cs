using System;
using System.Collections.Generic;

namespace Helpio.Ir.Domain.Entities.Core
{
    public class User : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<Ticketing.Ticket> AssignedTickets { get; set; } = new List<Ticketing.Ticket>();
        public virtual ICollection<Ticketing.Response> Responses { get; set; } = new List<Ticketing.Response>();
        public virtual ICollection<Ticketing.Note> Notes { get; set; } = new List<Ticketing.Note>();
    }
}