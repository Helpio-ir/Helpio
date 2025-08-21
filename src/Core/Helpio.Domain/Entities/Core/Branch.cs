using System;
using System.Collections.Generic;

namespace Helpio.Ir.Domain.Entities.Core
{
    public class Branch : BaseEntity
    {
        public int OrganizationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public int? BranchManagerId { get; set; }
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual Organization Organization { get; set; } = null!;
        public virtual User? BranchManager { get; set; }
        public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
    }
}