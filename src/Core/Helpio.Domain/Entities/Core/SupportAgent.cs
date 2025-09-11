using System;
using System.Collections.Generic;

namespace Helpio.Ir.Domain.Entities.Core
{
    public class SupportAgent : BaseEntity
    {
        public int? TeamId { get; set; }  // Changed to nullable
        public int UserId { get; set; }
        public int ProfileId { get; set; }
        public string AgentCode { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty; // e.g., Technical, Billing, General
        public int SupportLevel { get; set; } = 1; // 1=L1, 2=L2, 3=L3, etc.
        public decimal Salary { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsAvailable { get; set; } = true; // For ticket assignment
        public int MaxConcurrentTickets { get; set; } = 10;
        public int CurrentTicketCount { get; set; } = 0;
        
        // Navigation properties
        public virtual Team? Team { get; set; }  // Changed to nullable
        public virtual User User { get; set; } = null!;
        public virtual Profile Profile { get; set; } = null!;
        public virtual ICollection<Team> ManagedTeams { get; set; } = new List<Team>();
        public virtual ICollection<Team> SupervisedTeams { get; set; } = new List<Team>();
        public virtual ICollection<Ticketing.Ticket> AssignedTickets { get; set; } = new List<Ticketing.Ticket>();
        public virtual ICollection<Ticketing.Note> Notes { get; set; } = new List<Ticketing.Note>();
    }
}