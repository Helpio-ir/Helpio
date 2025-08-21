using Helpio.Ir.Application.DTOs.Common;

namespace Helpio.Ir.Application.DTOs.Core
{
    public class SupportAgentDto : BaseDto
    {
        public int TeamId { get; set; }
        public int UserId { get; set; }
        public string AgentCode { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public int SupportLevel { get; set; }
        public bool IsActive { get; set; }
        public bool IsAvailable { get; set; }
        public int MaxConcurrentTickets { get; set; }
        public int CurrentTicketCount { get; set; }
        
        // Navigation DTOs
        public UserDto? User { get; set; }
        public TeamDto? Team { get; set; }
        public ProfileDto? Profile { get; set; }
    }

    public class CreateSupportAgentDto
    {
        public int TeamId { get; set; }
        public int UserId { get; set; }
        public int ProfileId { get; set; }
        public string AgentCode { get; set; } = string.Empty;
        public DateTime HireDate { get; set; }
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public int SupportLevel { get; set; }
        public decimal Salary { get; set; }
        public int MaxConcurrentTickets { get; set; } = 10;
    }

    public class UpdateSupportAgentDto
    {
        public int TeamId { get; set; }
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public int SupportLevel { get; set; }
        public bool IsActive { get; set; }
        public bool IsAvailable { get; set; }
        public int MaxConcurrentTickets { get; set; }
    }
}