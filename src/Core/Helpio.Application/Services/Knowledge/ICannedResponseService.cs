using Helpio.Ir.Application.DTOs.Knowledge;
using Helpio.Ir.Application.DTOs.Common;

namespace Helpio.Ir.Application.Services.Knowledge
{
    public interface ICannedResponseService
    {
        Task<CannedResponseDto?> GetByIdAsync(int id);
        Task<CannedResponseDto?> GetByNameAsync(string name);
        Task<PaginatedResult<CannedResponseDto>> GetResponsesAsync(PaginationRequest request);
        Task<CannedResponseDto> CreateAsync(CreateCannedResponseDto createDto);
        Task<CannedResponseDto> UpdateAsync(int id, UpdateCannedResponseDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<CannedResponseDto>> GetByOrganizationIdAsync(int organizationId);
        Task<IEnumerable<CannedResponseDto>> GetActiveResponsesAsync();
        Task<IEnumerable<CannedResponseDto>> SearchByTagsAsync(string tags);
        Task<IEnumerable<CannedResponseDto>> GetMostUsedResponsesAsync(int count);
        Task<bool> IncrementUsageAsync(int responseId);
    }
}