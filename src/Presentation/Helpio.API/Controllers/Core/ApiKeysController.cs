using Microsoft.AspNetCore.Mvc;
using Helpio.Ir.Application.Services.Core;
using Helpio.Ir.Application.DTOs.Core;
using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.API.Services;
using FluentValidation;

namespace Helpio.Ir.API.Controllers.Core
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApiKeysController : ControllerBase
    {
        private readonly IApiKeyService _apiKeyService;
        private readonly IOrganizationContext _organizationContext;
        private readonly IValidator<CreateApiKeyDto> _createValidator;
        private readonly IValidator<UpdateApiKeyDto> _updateValidator;
        private readonly ILogger<ApiKeysController> _logger;

        public ApiKeysController(
            IApiKeyService apiKeyService,
            IOrganizationContext organizationContext,
            IValidator<CreateApiKeyDto> createValidator,
            IValidator<UpdateApiKeyDto> updateValidator,
            ILogger<ApiKeysController> logger)
        {
            _apiKeyService = apiKeyService;
            _organizationContext = organizationContext;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        /// <summary>
        /// Get all API keys for the authenticated organization
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<ApiKeyDto>>> GetApiKeys([FromQuery] PaginationRequest request)
        {
            if (!_organizationContext.OrganizationId.HasValue)
            {
                return BadRequest("Organization context not found");
            }

            // Filter by organization
            var apiKeys = await _apiKeyService.GetByOrganizationIdAsync(_organizationContext.OrganizationId.Value);
            
            // Apply pagination manually (could be moved to service layer)
            var totalItems = apiKeys.Count();
            var items = apiKeys
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var result = new PaginatedResult<ApiKeyDto>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return Ok(result);
        }

        /// <summary>
        /// Get API key by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiKeyDto>> GetApiKey(int id)
        {
            var apiKey = await _apiKeyService.GetByIdAsync(id);
            if (apiKey == null)
            {
                return NotFound();
            }

            // Ensure API key belongs to the authenticated organization
            if (apiKey.OrganizationId != _organizationContext.OrganizationId)
            {
                return NotFound();
            }

            return Ok(apiKey);
        }

        /// <summary>
        /// Create a new API key
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiKeyResponseDto>> CreateApiKey(CreateApiKeyDto createDto)
        {
            // Validate input
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            // Ensure API key is created for the authenticated organization
            if (!_organizationContext.OrganizationId.HasValue)
            {
                return BadRequest("Organization context not found");
            }

            createDto.OrganizationId = _organizationContext.OrganizationId.Value;

            try
            {
                var result = await _apiKeyService.CreateAsync(createDto);
                
                _logger.LogInformation("API Key created: {KeyName} for Organization: {OrganizationId}", 
                    result.KeyName, createDto.OrganizationId);

                return CreatedAtAction(nameof(GetApiKey), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating API key for Organization: {OrganizationId}", createDto.OrganizationId);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update API key
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiKeyDto>> UpdateApiKey(int id, UpdateApiKeyDto updateDto)
        {
            // Validate input
            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            // Check if API key exists and belongs to organization
            var existingApiKey = await _apiKeyService.GetByIdAsync(id);
            if (existingApiKey == null)
            {
                return NotFound();
            }

            if (existingApiKey.OrganizationId != _organizationContext.OrganizationId)
            {
                return NotFound();
            }

            try
            {
                var result = await _apiKeyService.UpdateAsync(id, updateDto);
                
                _logger.LogInformation("API Key updated: {KeyId}", id);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating API key: {KeyId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete API key
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApiKey(int id)
        {
            // Check if API key exists and belongs to organization
            var existingApiKey = await _apiKeyService.GetByIdAsync(id);
            if (existingApiKey == null)
            {
                return NotFound();
            }

            if (existingApiKey.OrganizationId != _organizationContext.OrganizationId)
            {
                return NotFound();
            }

            try
            {
                var result = await _apiKeyService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                _logger.LogInformation("API Key deleted: {KeyId}", id);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting API key: {KeyId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Revoke API key (set as inactive)
        /// </summary>
        [HttpPost("{id}/revoke")]
        public async Task<IActionResult> RevokeApiKey(int id)
        {
            // Check if API key exists and belongs to organization
            var existingApiKey = await _apiKeyService.GetByIdAsync(id);
            if (existingApiKey == null)
            {
                return NotFound();
            }

            if (existingApiKey.OrganizationId != _organizationContext.OrganizationId)
            {
                return NotFound();
            }

            try
            {
                var result = await _apiKeyService.RevokeApiKeyAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                _logger.LogInformation("API Key revoked: {KeyId}", id);
                
                return Ok(new { message = "API Key revoked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking API key: {KeyId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Regenerate API key (creates new key value)
        /// </summary>
        [HttpPost("{id}/regenerate")]
        public async Task<IActionResult> RegenerateApiKey(int id)
        {
            // Check if API key exists and belongs to organization
            var existingApiKey = await _apiKeyService.GetByIdAsync(id);
            if (existingApiKey == null)
            {
                return NotFound();
            }

            if (existingApiKey.OrganizationId != _organizationContext.OrganizationId)
            {
                return NotFound();
            }

            try
            {
                var result = await _apiKeyService.RegenerateApiKeyAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                _logger.LogInformation("API Key regenerated: {KeyId}", id);
                
                return Ok(new { message = "API Key regenerated successfully. Please retrieve the new key value." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error regenerating API key: {KeyId}", id);
                return BadRequest(ex.Message);
            }
        }
    }
}