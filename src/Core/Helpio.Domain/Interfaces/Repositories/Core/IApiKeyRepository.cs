using Helpio.Ir.Domain.Entities.Core;

namespace Helpio.Ir.Domain.Interfaces.Repositories.Core
{
    public interface IApiKeyRepository : IRepository<ApiKey>
    {
        Task<ApiKey?> GetByKeyValueAsync(string keyValue);
        Task<ApiKey?> GetByKeyHashAsync(string keyHash);
        Task<IEnumerable<ApiKey>> GetByOrganizationIdAsync(int organizationId);
        Task<IEnumerable<ApiKey>> GetActiveKeysAsync();
        Task<IEnumerable<ApiKey>> GetExpiredKeysAsync();
        Task<bool> IsKeyValidAsync(string keyValue);
        Task<bool> UpdateLastUsedAsync(int keyId);
        Task<ApiKey?> GetValidKeyAsync(string keyValue, string? clientIp = null);
    }
}