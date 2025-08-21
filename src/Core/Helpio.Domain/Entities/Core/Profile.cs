using System;

namespace Helpio.Ir.Domain.Entities.Core
{
    public class Profile : BaseEntity
    {
        public string Avatar { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string Skills { get; set; } = string.Empty;
        public string Certifications { get; set; } = string.Empty;
        public DateTime? LastLoginDate { get; set; }
        
        // Navigation properties
        public virtual SupportAgent? SupportAgent { get; set; }
    }
}