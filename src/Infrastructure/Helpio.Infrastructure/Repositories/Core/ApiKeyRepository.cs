using Microsoft.EntityFrameworkCore;
using Helpio.Ir.Domain.Entities.Core;
using Helpio.Ir.Domain.Interfaces.Repositories.Core;
using Helpio.Ir.Infrastructure.Data;
using System.Security.Cryptography;
using System.Text;

namespace Helpio.Ir.Infrastructure.Repositories.Core
{
    public class ApiKeyRepository : Repository<ApiKey>, IApiKeyRepository
    {
        public ApiKeyRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ApiKey?> GetByKeyValueAsync(string keyValue)
        {
            return await _dbSet
                .Include(ak => ak.Organization)
                .FirstOrDefaultAsync(ak => ak.KeyValue == keyValue);
        }

        public async Task<ApiKey?> GetByKeyHashAsync(string keyHash)
        {
            return await _dbSet
                .Include(ak => ak.Organization)
                .FirstOrDefaultAsync(ak => ak.KeyHash == keyHash);
        }

        public async Task<IEnumerable<ApiKey>> GetByOrganizationIdAsync(int organizationId)
        {
            return await _dbSet
                .Include(ak => ak.Organization)
                .Where(ak => ak.OrganizationId == organizationId && !ak.IsDeleted)
                .OrderByDescending(ak => ak.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ApiKey>> GetActiveKeysAsync()
        {
            return await _dbSet
                .Include(ak => ak.Organization)
                .Where(ak => ak.IsActive && !ak.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<ApiKey>> GetExpiredKeysAsync()
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Include(ak => ak.Organization)
                .Where(ak => ak.ExpiresAt.HasValue && ak.ExpiresAt.Value < now && !ak.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> IsKeyValidAsync(string keyValue)
        {
            var now = DateTime.UtcNow;
            return await _dbSet.AnyAsync(ak => 
                ak.KeyValue == keyValue && 
                ak.IsActive && 
                !ak.IsDeleted &&
                (!ak.ExpiresAt.HasValue || ak.ExpiresAt.Value > now));
        }

        public async Task<bool> UpdateLastUsedAsync(int keyId)
        {
            var apiKey = await _dbSet.FindAsync(keyId);
            if (apiKey == null) return false;

            apiKey.LastUsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ApiKey?> GetValidKeyAsync(string keyValue, string? clientIp = null)
        {
            var now = DateTime.UtcNow;
            var query = _dbSet
                .Include(ak => ak.Organization)
                .Where(ak => 
                    ak.KeyValue == keyValue && 
                    ak.IsActive && 
                    !ak.IsDeleted &&
                    (!ak.ExpiresAt.HasValue || ak.ExpiresAt.Value > now));

            var apiKey = await query.FirstOrDefaultAsync();
            
            if (apiKey == null) return null;

            // Check IP restrictions if specified
            if (!string.IsNullOrEmpty(apiKey.AllowedIPs) && !string.IsNullOrEmpty(clientIp))
            {
                var allowedIPs = apiKey.AllowedIPs.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(ip => ip.Trim());
                
                if (!allowedIPs.Contains(clientIp))
                {
                    return null; // IP not allowed
                }
            }

            return apiKey;
        }
    }
}