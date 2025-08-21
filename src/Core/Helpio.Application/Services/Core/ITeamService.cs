using Helpio.Ir.Application.DTOs.Core;
using Helpio.Ir.Application.DTOs.Common;

namespace Helpio.Ir.Application.Services.Core
{
    public interface ITeamService
    {
        Task<TeamDto?> GetByIdAsync(int id);
        Task<TeamDto?> GetWithSupportAgentsAsync(int id);
        Task<PaginatedResult<TeamDto>> GetTeamsAsync(PaginationRequest request);
        Task<TeamDto> CreateAsync(CreateTeamDto createDto);
        Task<TeamDto> UpdateAsync(int id, UpdateTeamDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<TeamDto>> GetByBranchIdAsync(int branchId);
        Task<IEnumerable<TeamDto>> GetActiveTeamsAsync();
        Task<IEnumerable<TeamDto>> GetTeamsByManagerAsync(int managerId);
    }
}