using AutoMapper;
using Microsoft.Extensions.Logging;
using Helpio.Ir.Application.DTOs.Core;
using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.Application.Services.Core;
using Helpio.Ir.Application.Common.Exceptions;
using Helpio.Ir.Domain.Interfaces;
using Helpio.Ir.Domain.Entities.Core;
using System.Security.Cryptography;
using System.Text;

namespace Helpio.Ir.Application.Services.Core
{
    public class ApiKeyService : IApiKeyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ApiKeyService> _logger;

        public ApiKeyService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ApiKeyService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiKeyDto?> GetByIdAsync(int id)
        {
            var apiKey = await _unitOfWork.ApiKeys.GetByIdAsync(id);
            if (apiKey == null) return null;

            var dto = _mapper.Map<ApiKeyDto>(apiKey);
            // Mask the key value for security
            dto.KeyValue = dto.MaskedKey;
            return dto;
        }

        public async Task<PaginatedResult<ApiKeyDto>> GetApiKeysAsync(PaginationRequest request)
        {
            var apiKeys = await _unitOfWork.ApiKeys.GetAllAsync();

            // Apply search filter
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                apiKeys = apiKeys.Where(k =>
                    k.KeyName.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (k.Description != null && k.Description.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            // Apply sorting
            apiKeys = request.SortBy?.ToLower() switch
            {
                "keyname" => request.SortDescending ? apiKeys.OrderByDescending(k => k.KeyName) : apiKeys.OrderBy(k => k.KeyName),
                "createdat" => request.SortDescending ? apiKeys.OrderByDescending(k => k.CreatedAt) : apiKeys.OrderBy(k => k.CreatedAt),
                "expiresat" => request.SortDescending ? apiKeys.OrderByDescending(k => k.ExpiresAt) : apiKeys.OrderBy(k => k.ExpiresAt),
                "lastused" => request.SortDescending ? apiKeys.OrderByDescending(k => k.LastUsedAt) : apiKeys.OrderBy(k => k.LastUsedAt),
                _ => apiKeys.OrderByDescending(k => k.CreatedAt)
            };

            var totalItems = apiKeys.Count();
            var items = apiKeys
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var apiKeyDtos = _mapper.Map<List<ApiKeyDto>>(items);
            
            // Mask all key values for security
            foreach (var dto in apiKeyDtos)
            {
                dto.KeyValue = dto.MaskedKey;
            }

            return new PaginatedResult<ApiKeyDto>
            {
                Items = apiKeyDtos,
                TotalItems = totalItems,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<ApiKeyResponseDto> CreateAsync(CreateApiKeyDto createDto)
        {
            // Validate organization exists
            var organization = await _unitOfWork.Organizations.GetByIdAsync(createDto.OrganizationId);
            if (organization == null)
            {
                throw new NotFoundException("Organization", createDto.OrganizationId);
            }

            // Generate API key
            var keyValue = GenerateApiKey();
            var keyHash = HashApiKey(keyValue);

            var apiKey = _mapper.Map<ApiKey>(createDto);
            apiKey.KeyValue = keyValue;
            apiKey.KeyHash = keyHash;

            var createdApiKey = await _unitOfWork.ApiKeys.AddAsync(apiKey);

            _logger.LogInformation("API Key created with ID: {ApiKeyId}, Name: {KeyName} for Organization: {OrganizationId}", 
                createdApiKey.Id, createdApiKey.KeyName, createDto.OrganizationId);

            return new ApiKeyResponseDto
            {
                Id = createdApiKey.Id,
                KeyName = createdApiKey.KeyName,
                KeyValue = keyValue, // Return the actual key only once
                Message = "API Key created successfully. Please store this key securely as it won't be shown again."
            };
        }

        public async Task<ApiKeyDto> UpdateAsync(int id, UpdateApiKeyDto updateDto)
        {
            var apiKey = await _unitOfWork.ApiKeys.GetByIdAsync(id);
            if (apiKey == null)
            {
                throw new NotFoundException("ApiKey", id);
            }

            _mapper.Map(updateDto, apiKey);
            await _unitOfWork.ApiKeys.UpdateAsync(apiKey);

            _logger.LogInformation("API Key updated with ID: {ApiKeyId}", id);

            var dto = _mapper.Map<ApiKeyDto>(apiKey);
            dto.KeyValue = dto.MaskedKey; // Mask the key value
            return dto;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var apiKey = await _unitOfWork.ApiKeys.GetByIdAsync(id);
            if (apiKey == null)
            {
                return false;
            }

            await _unitOfWork.ApiKeys.DeleteAsync(apiKey);

            _logger.LogInformation("API Key deleted with ID: {ApiKeyId}", id);

            return true;
        }

        public async Task<IEnumerable<ApiKeyDto>> GetByOrganizationIdAsync(int organizationId)
        {
            var apiKeys = await _unitOfWork.ApiKeys.GetByOrganizationIdAsync(organizationId);
            var dtos = _mapper.Map<IEnumerable<ApiKeyDto>>(apiKeys);
            
            // Mask all key values
            foreach (var dto in dtos)
            {
                dto.KeyValue = dto.MaskedKey;
            }
            
            return dtos;
        }

        public async Task<bool> ValidateApiKeyAsync(string keyValue, string? clientIp = null)
        {
            var apiKey = await _unitOfWork.ApiKeys.GetValidKeyAsync(keyValue, clientIp);
            
            if (apiKey != null)
            {
                // Update last used timestamp
                await _unitOfWork.ApiKeys.UpdateLastUsedAsync(apiKey.Id);
                return true;
            }
            
            return false;
        }

        public async Task<ApiKeyDto?> GetByKeyValueAsync(string keyValue)
        {
            var apiKey = await _unitOfWork.ApiKeys.GetByKeyValueAsync(keyValue);
            if (apiKey == null) return null;

            var dto = _mapper.Map<ApiKeyDto>(apiKey);
            dto.KeyValue = dto.MaskedKey; // Mask the key value
            return dto;
        }

        public async Task<bool> RevokeApiKeyAsync(int id)
        {
            var apiKey = await _unitOfWork.ApiKeys.GetByIdAsync(id);
            if (apiKey == null)
            {
                throw new NotFoundException("ApiKey", id);
            }

            apiKey.IsActive = false;
            await _unitOfWork.ApiKeys.UpdateAsync(apiKey);

            _logger.LogInformation("API Key revoked with ID: {ApiKeyId}", id);

            return true;
        }

        public async Task<bool> RegenerateApiKeyAsync(int id)
        {
            var apiKey = await _unitOfWork.ApiKeys.GetByIdAsync(id);
            if (apiKey == null)
            {
                throw new NotFoundException("ApiKey", id);
            }

            // Generate new key
            var newKeyValue = GenerateApiKey();
            var newKeyHash = HashApiKey(newKeyValue);

            apiKey.KeyValue = newKeyValue;
            apiKey.KeyHash = newKeyHash;
            await _unitOfWork.ApiKeys.UpdateAsync(apiKey);

            _logger.LogInformation("API Key regenerated with ID: {ApiKeyId}", id);

            return true;
        }

        public async Task<IEnumerable<ApiKeyDto>> GetExpiredKeysAsync()
        {
            var expiredKeys = await _unitOfWork.ApiKeys.GetExpiredKeysAsync();
            var dtos = _mapper.Map<IEnumerable<ApiKeyDto>>(expiredKeys);
            
            // Mask all key values
            foreach (var dto in dtos)
            {
                dto.KeyValue = dto.MaskedKey;
            }
            
            return dtos;
        }

        public async Task<bool> UpdateLastUsedAsync(string keyValue)
        {
            var apiKey = await _unitOfWork.ApiKeys.GetByKeyValueAsync(keyValue);
            if (apiKey == null) return false;

            return await _unitOfWork.ApiKeys.UpdateLastUsedAsync(apiKey.Id);
        }

        #region Private Methods

        private static string GenerateApiKey()
        {
            // Generate a secure random API key
            const string prefix = "hk_"; // helpio key
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var randomPart = Convert.ToBase64String(randomBytes)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
            
            return $"{prefix}{randomPart}";
        }

        private static string HashApiKey(string keyValue)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(keyValue + "HELPIO_SALT"));
            return Convert.ToBase64String(hashedBytes);
        }

        #endregion
    }
}