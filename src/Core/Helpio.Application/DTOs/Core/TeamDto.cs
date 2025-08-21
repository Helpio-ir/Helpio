using Helpio.Ir.Application.DTOs.Common;

namespace Helpio.Ir.Application.DTOs.Core
{
    public class TeamDto : BaseDto
    {
        public int BranchId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? TeamLeadId { get; set; }
        public int? SupervisorId { get; set; }
        public bool IsActive { get; set; }
        
        // Navigation DTOs
        public BranchDto? Branch { get; set; }
        public SupportAgentDto? TeamLead { get; set; }
        public SupportAgentDto? Supervisor { get; set; }
        public int AgentCount { get; set; }
        public int ActiveTicketCount { get; set; }
    }

    public class CreateTeamDto
    {
        public int BranchId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? TeamLeadId { get; set; }
        public int? SupervisorId { get; set; }
    }

    public class UpdateTeamDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? TeamLeadId { get; set; }
        public int? SupervisorId { get; set; }
        public bool IsActive { get; set; }
    }
}