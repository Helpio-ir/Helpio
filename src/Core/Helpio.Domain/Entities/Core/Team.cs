using System;
using System.Collections.Generic;

namespace Helpio.Ir.Domain.Entities.Core
{
    public class Team : BaseEntity
    {
        public int BranchId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? TeamLeadId { get; set; }
        public int? SupervisorId { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual Branch Branch { get; set; } = null!;
        public virtual SupportAgent? TeamLead { get; set; }
        public virtual SupportAgent? Supervisor { get; set; }
        public virtual ICollection<SupportAgent> SupportAgents { get; set; } = new List<SupportAgent>();
        public virtual ICollection<Ticketing.Ticket> Tickets { get; set; } = new List<Ticketing.Ticket>();
    }
}