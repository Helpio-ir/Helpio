using Microsoft.AspNetCore.Mvc;
using Helpio.Ir.Application.Services.Knowledge;
using Helpio.Ir.Application.DTOs.Knowledge;
using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.API.Services;
using FluentValidation;

namespace Helpio.Ir.API.Controllers.Knowledge
{
    [ApiController]
    [Route("api/[controller]")]
    public class CannedResponsesController : ControllerBase
    {
        private readonly ICannedResponseService _cannedResponseService;
        private readonly IOrganizationContext _organizationContext;
        private readonly IValidator<CreateCannedResponseDto> _createValidator;
        private readonly IValidator<UpdateCannedResponseDto> _updateValidator;
        private readonly ILogger<CannedResponsesController> _logger;

        public CannedResponsesController(
            ICannedResponseService cannedResponseService,
            IOrganizationContext organizationContext,
            IValidator<CreateCannedResponseDto> createValidator,
            IValidator<UpdateCannedResponseDto> updateValidator,
            ILogger<CannedResponsesController> logger)
        {
            _cannedResponseService = cannedResponseService;
            _organizationContext = organizationContext;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        /// <summary>
        /// Get all canned responses with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<CannedResponseDto>>> GetCannedResponses([FromQuery] PaginationRequest request)
        {
            try
            {
                var result = await _cannedResponseService.GetResponsesAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving canned responses");
                return BadRequest("Error retrieving canned responses");
            }
        }

        /// <summary>
        /// Get canned response by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CannedResponseDto>> GetCannedResponse(int id)
        {
            var response = await _cannedResponseService.GetByIdAsync(id);
            if (response == null)
            {
                return NotFound();
            }

            // ????? ?????? ???????
            if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
            {
                if (response.OrganizationId != _organizationContext.OrganizationId.Value)
                {
                    return Forbid("Access denied to other organization's canned responses");
                }
            }
            // ??? ????? ????? ???? ????? ??? ???????? ???? ???? ?????? ???
            else if (!response.IsActive)
            {
                return NotFound();
            }

            return Ok(response);
        }

        /// <summary>
        /// Get canned response by name
        /// </summary>
        [HttpGet("by-name/{name}")]
        public async Task<ActionResult<CannedResponseDto>> GetCannedResponseByName(string name)
        {
            var response = await _cannedResponseService.GetByNameAsync(name);
            if (response == null)
            {
                return NotFound();
            }

            // ????? ?????? ???????
            if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
            {
                if (response.OrganizationId != _organizationContext.OrganizationId.Value)
                {
                    return Forbid("Access denied to other organization's canned responses");
                }
            }
            else if (!response.IsActive)
            {
                return NotFound();
            }

            return Ok(response);
        }

        /// <summary>
        /// Create a new canned response
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CannedResponseDto>> CreateCannedResponse(CreateCannedResponseDto createDto)
        {
            // Validate input
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            // Ensure canned response is created for the authenticated organization
            if (_organizationContext.OrganizationId.HasValue)
            {
                createDto.OrganizationId = _organizationContext.OrganizationId.Value;
            }

            try
            {
                var result = await _cannedResponseService.CreateAsync(createDto);
                
                _logger.LogInformation("Canned response created: {ResponseName} for Organization: {OrganizationId}", 
                    result.Name, createDto.OrganizationId);

                return CreatedAtAction(nameof(GetCannedResponse), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Duplicate canned response name: {ResponseName}", createDto.Name);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating canned response: {ResponseName}", createDto.Name);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update canned response
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<CannedResponseDto>> UpdateCannedResponse(int id, UpdateCannedResponseDto updateDto)
        {
            // ????? ????? ????
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            if (!_organizationContext.OrganizationId.HasValue)
            {
                return BadRequest("Organization context not found");
            }

            // ????? ???? ???? ????? ? ?????? ???????
            var existingResponse = await _cannedResponseService.GetByIdAsync(id);
            if (existingResponse == null)
            {
                return NotFound();
            }

            if (existingResponse.OrganizationId != _organizationContext.OrganizationId.Value)
            {
                return Forbid("Access denied to other organization's canned responses");
            }

            // Validate input
            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _cannedResponseService.UpdateAsync(id, updateDto);
                
                _logger.LogInformation("Canned response updated: {ResponseId}", id);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating canned response: {ResponseId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete canned response
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCannedResponse(int id)
        {
            // ????? ????? ????
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            if (!_organizationContext.OrganizationId.HasValue)
            {
                return BadRequest("Organization context not found");
            }

            // ????? ???? ???? ????? ? ?????? ???????
            var existingResponse = await _cannedResponseService.GetByIdAsync(id);
            if (existingResponse == null)
            {
                return NotFound();
            }

            if (existingResponse.OrganizationId != _organizationContext.OrganizationId.Value)
            {
                return Forbid("Access denied to other organization's canned responses");
            }

            try
            {
                var result = await _cannedResponseService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                _logger.LogInformation("Canned response deleted: {ResponseId}", id);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting canned response: {ResponseId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get canned responses by organization ID
        /// </summary>
        [HttpGet("organization/{organizationId}")]
        public async Task<ActionResult<IEnumerable<CannedResponseDto>>> GetCannedResponsesByOrganization(int organizationId)
        {
            // Authenticate user first
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            // Check if OrganizationId is valid
            if (!_organizationContext.OrganizationId.HasValue)
            {
                return BadRequest("Organization context not found");
            }

            // Ensure user can only access their organization's canned responses
            if (_organizationContext.OrganizationId.Value != organizationId)
            {
                return Forbid("Access denied to other organization's canned responses");
            }

            try
            {
                var responses = await _cannedResponseService.GetByOrganizationIdAsync(organizationId);
                return Ok(responses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving canned responses for organization: {OrganizationId}", organizationId);
                return BadRequest("Error retrieving canned responses");
            }
        }

        /// <summary>
        /// Get canned responses for authenticated organization
        /// </summary>
        [HttpGet("my-organization")]
        public async Task<ActionResult<IEnumerable<CannedResponseDto>>> GetMyOrganizationCannedResponses()
        {
            if (!_organizationContext.OrganizationId.HasValue)
            {
                return BadRequest("Organization context not found");
            }

            try
            {
                var responses = await _cannedResponseService.GetByOrganizationIdAsync(_organizationContext.OrganizationId.Value);
                return Ok(responses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving canned responses for organization: {OrganizationId}", _organizationContext.OrganizationId);
                return BadRequest("Error retrieving canned responses");
            }
        }

        /// <summary>
        /// Get active canned responses
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<CannedResponseDto>>> GetActiveCannedResponses()
        {
            try
            {
                var responses = await _cannedResponseService.GetActiveResponsesAsync();
                return Ok(responses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active canned responses");
                return BadRequest("Error retrieving active canned responses");
            }
        }

        /// <summary>
        /// Search canned responses by tags
        /// </summary>
        [HttpGet("search/tags")]
        public async Task<ActionResult<IEnumerable<CannedResponseDto>>> SearchCannedResponsesByTags([FromQuery] string tags)
        {
            if (string.IsNullOrWhiteSpace(tags))
            {
                return BadRequest("Tags parameter is required");
            }

            try
            {
                var responses = await _cannedResponseService.SearchByTagsAsync(tags);
                return Ok(responses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching canned responses by tags: {Tags}", tags);
                return BadRequest("Error searching canned responses");
            }
        }

        /// <summary>
        /// Get most used canned responses
        /// </summary>
        [HttpGet("most-used")]
        public async Task<ActionResult<IEnumerable<CannedResponseDto>>> GetMostUsedCannedResponses([FromQuery] int count = 10)
        {
            if (count <= 0 || count > 100)
            {
                return BadRequest("Count must be between 1 and 100");
            }

            try
            {
                var responses = await _cannedResponseService.GetMostUsedResponsesAsync(count);
                return Ok(responses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving most used canned responses");
                return BadRequest("Error retrieving most used canned responses");
            }
        }

        /// <summary>
        /// Use a canned response (increment usage count)
        /// </summary>
        [HttpPost("{id}/use")]
        public async Task<IActionResult> UseCannedResponse(int id)
        {
            // ????? ???? ???? ?????
            var existingResponse = await _cannedResponseService.GetByIdAsync(id);
            if (existingResponse == null)
            {
                return NotFound();
            }

            // ????? ?????? ??????? - ??? ????? ????? ???? ???
            if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
            {
                if (existingResponse.OrganizationId != _organizationContext.OrganizationId.Value)
                {
                    return Forbid("Access denied to other organization's canned responses");
                }
            }
            // ??? ????? ????? ???? ????? ??? ???????? ???? ???? ??????? ???
            else if (!existingResponse.IsActive)
            {
                return NotFound();
            }

            try
            {
                var result = await _cannedResponseService.IncrementUsageAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                _logger.LogInformation("Canned response {ResponseId} usage incremented", id);
                
                return Ok(new { message = "Usage count incremented successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing usage for canned response: {ResponseId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get canned response statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetCannedResponseStatistics()
        {
            try
            {
                // Get responses for the organization if context is available
                IEnumerable<CannedResponseDto> responses;
                if (_organizationContext.OrganizationId.HasValue)
                {
                    responses = await _cannedResponseService.GetByOrganizationIdAsync(_organizationContext.OrganizationId.Value);
                }
                else
                {
                    // Get all active responses if no organization context
                    responses = await _cannedResponseService.GetActiveResponsesAsync();
                }

                var statistics = new
                {
                    TotalResponses = responses.Count(),
                    ActiveResponses = responses.Count(r => r.IsActive),
                    InactiveResponses = responses.Count(r => !r.IsActive),
                    TotalUsage = responses.Sum(r => r.UsageCount),
                    AverageUsage = responses.Any() ? responses.Average(r => r.UsageCount) : 0,
                    MostUsedResponse = responses.OrderByDescending(r => r.UsageCount).FirstOrDefault(),
                    UnusedResponses = responses.Count(r => r.UsageCount == 0),
                    ResponsesWithTags = responses.Count(r => !string.IsNullOrEmpty(r.Tags))
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving canned response statistics");
                return BadRequest("Error retrieving statistics");
            }
        }

        /// <summary>
        /// Get usage analytics
        /// </summary>
        [HttpGet("analytics/usage")]
        public async Task<IActionResult> GetUsageAnalytics()
        {
            try
            {
                // Get responses for the organization if context is available
                IEnumerable<CannedResponseDto> responses;
                if (_organizationContext.OrganizationId.HasValue)
                {
                    responses = await _cannedResponseService.GetByOrganizationIdAsync(_organizationContext.OrganizationId.Value);
                }
                else
                {
                    responses = await _cannedResponseService.GetActiveResponsesAsync();
                }

                var analytics = new
                {
                    TopUsedResponses = responses
                        .OrderByDescending(r => r.UsageCount)
                        .Take(10)
                        .Select(r => new { r.Id, r.Name, r.UsageCount }),
                    
                    UsageDistribution = new
                    {
                        HighUsage = responses.Count(r => r.UsageCount >= 50),
                        MediumUsage = responses.Count(r => r.UsageCount >= 10 && r.UsageCount < 50),
                        LowUsage = responses.Count(r => r.UsageCount > 0 && r.UsageCount < 10),
                        NoUsage = responses.Count(r => r.UsageCount == 0)
                    },
                    
                    TagAnalytics = responses
                        .Where(r => !string.IsNullOrEmpty(r.Tags))
                        .SelectMany(r => r.TagList)
                        .GroupBy(tag => tag)
                        .OrderByDescending(g => g.Count())
                        .Take(10)
                        .Select(g => new { Tag = g.Key, Count = g.Count() })
                };

                return Ok(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving usage analytics");
                return BadRequest("Error retrieving analytics");
            }
        }
    }
}