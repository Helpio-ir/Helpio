using Helpio.Ir.Application.DTOs.Core;
using Helpio.Ir.Application.DTOs.Common;

namespace Helpio.Ir.Application.Services.Core
{
    public interface ISupportAgentService
    {
        Task<SupportAgentDto?> GetByIdAsync(int id);
        Task<SupportAgentDto?> GetByAgentCodeAsync(string agentCode);
        Task<PaginatedResult<SupportAgentDto>> GetAgentsAsync(PaginationRequest request);
        Task<SupportAgentDto> CreateAsync(CreateSupportAgentDto createDto);
        Task<SupportAgentDto> UpdateAsync(int id, UpdateSupportAgentDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<SupportAgentDto>> GetByTeamIdAsync(int teamId);
        Task<IEnumerable<SupportAgentDto>> GetAvailableAgentsAsync();
        Task<IEnumerable<SupportAgentDto>> GetBySpecializationAsync(string specialization);
        Task<IEnumerable<SupportAgentDto>> GetAgentsWithLowWorkloadAsync(int maxTickets);
        Task<bool> SetAvailabilityAsync(int agentId, bool isAvailable);
        Task<bool> UpdateWorkloadAsync(int agentId, int ticketCount);
        Task<SupportAgentDto?> GetBestAvailableAgentAsync(int teamId, string? specialization = null);
    }
}