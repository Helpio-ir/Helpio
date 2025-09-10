using Helpio.Ir.Application.DTOs.Core;
using Helpio.Ir.Application.DTOs.Common;

namespace Helpio.Ir.Application.Services.Core
{
    public interface IApiKeyService
    {
        Task<ApiKeyDto?> GetByIdAsync(int id);
        Task<PaginatedResult<ApiKeyDto>> GetApiKeysAsync(PaginationRequest request);
        Task<ApiKeyResponseDto> CreateAsync(CreateApiKeyDto createDto);
        Task<ApiKeyDto> UpdateAsync(int id, UpdateApiKeyDto updateDto);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<ApiKeyDto>> GetByOrganizationIdAsync(int organizationId);
        Task<bool> ValidateApiKeyAsync(string keyValue, string? clientIp = null);
        Task<ApiKeyDto?> GetByKeyValueAsync(string keyValue);
        Task<bool> RevokeApiKeyAsync(int id);
        Task<bool> RegenerateApiKeyAsync(int id);
        Task<IEnumerable<ApiKeyDto>> GetExpiredKeysAsync();
        Task<bool> UpdateLastUsedAsync(string keyValue);
    }
}