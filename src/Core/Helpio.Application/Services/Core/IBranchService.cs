using Helpio.Ir.Application.DTOs.Core;
using Helpio.Ir.Application.DTOs.Common;

namespace Helpio.Ir.Application.Services.Core
{
    public interface IBranchService
    {
        Task<BranchDto?> GetByIdAsync(int id);
        Task<BranchDto?> GetWithTeamsAsync(int id);
        Task<PaginatedResult<BranchDto>> GetBranchesAsync(PaginationRequest request);
        Task<BranchDto> CreateAsync(CreateBranchDto createDto);
        Task<BranchDto> UpdateAsync(int id, UpdateBranchDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<BranchDto>> GetByOrganizationIdAsync(int organizationId);
        Task<IEnumerable<BranchDto>> GetActiveBranchesAsync();
    }
}