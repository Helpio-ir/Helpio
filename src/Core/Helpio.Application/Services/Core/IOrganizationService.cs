using Helpio.Ir.Application.DTOs.Core;
using Helpio.Ir.Application.DTOs.Common;

namespace Helpio.Ir.Application.Services.Core
{
    public interface IOrganizationService
    {
        Task<OrganizationDto?> GetByIdAsync(int id);
        Task<OrganizationDto?> GetWithBranchesAsync(int id);
        Task<OrganizationDto?> GetWithTicketCategoriesAsync(int id);
        Task<PaginatedResult<OrganizationDto>> GetOrganizationsAsync(PaginationRequest request);
        Task<OrganizationDto> CreateAsync(CreateOrganizationDto createDto);
        Task<OrganizationDto> UpdateAsync(int id, UpdateOrganizationDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<OrganizationDto>> GetActiveOrganizationsAsync();
    }
}