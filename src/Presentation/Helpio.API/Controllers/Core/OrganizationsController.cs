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
    public class OrganizationsController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;
        private readonly IOrganizationContext _organizationContext;
        private readonly IValidator<CreateOrganizationDto> _createValidator;
        private readonly IValidator<UpdateOrganizationDto> _updateValidator;
        private readonly ILogger<OrganizationsController> _logger;

        public OrganizationsController(
            IOrganizationService organizationService,
            IOrganizationContext organizationContext,
            IValidator<CreateOrganizationDto> createValidator,
            IValidator<UpdateOrganizationDto> updateValidator,
            ILogger<OrganizationsController> logger)
        {
            _organizationService = organizationService;
            _organizationContext = organizationContext;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        /// <summary>
        /// Get all organizations with pagination (Admin only)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<OrganizationDto>>> GetOrganizations([FromQuery] PaginationRequest request)
        {
            // ??? endpoint ??? ???? admins ???
            // ?? ?? ??????? ?????? ???? role-based authorization ????? ???
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            try
            {
                var result = await _organizationService.GetOrganizationsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving organizations");
                return BadRequest("Error retrieving organizations");
            }
        }

        /// <summary>
        /// Get organization by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<OrganizationDto>> GetOrganization(int id)
        {
            var organization = await _organizationService.GetByIdAsync(id);
            if (organization == null)
            {
                return NotFound();
            }

            // ????? ?????? - ??????? ??? ????????? ?????? ?????? ?? ??????
            if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
            {
                if (_organizationContext.OrganizationId.Value != id)
                {
                    return Forbid("Access denied to other organizations");
                }
            }

            return Ok(organization);
        }

        /// <summary>
        /// Get current authenticated organization
        /// </summary>
        [HttpGet("current")]
        public async Task<ActionResult<OrganizationDto>> GetCurrentOrganization()
        {
            if (!_organizationContext.IsAuthenticated || !_organizationContext.OrganizationId.HasValue)
            {
                return BadRequest("Organization context not found");
            }

            var organization = await _organizationService.GetByIdAsync(_organizationContext.OrganizationId.Value);
            if (organization == null)
            {
                return NotFound();
            }

            return Ok(organization);
        }

        /// <summary>
        /// Get organization with branches
        /// </summary>
        [HttpGet("{id}/branches")]
        public async Task<ActionResult<OrganizationDto>> GetOrganizationWithBranches(int id)
        {
            // ????? ??????
            if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
            {
                if (_organizationContext.OrganizationId.Value != id)
                {
                    return Forbid("Access denied to other organizations");
                }
            }

            var organization = await _organizationService.GetWithBranchesAsync(id);
            if (organization == null)
            {
                return NotFound();
            }

            return Ok(organization);
        }

        /// <summary>
        /// Get organization with ticket categories
        /// </summary>
        [HttpGet("{id}/categories")]
        public async Task<ActionResult<OrganizationDto>> GetOrganizationWithCategories(int id)
        {
            // ????? ??????
            if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
            {
                if (_organizationContext.OrganizationId.Value != id)
                {
                    return Forbid("Access denied to other organizations");
                }
            }

            var organization = await _organizationService.GetWithTicketCategoriesAsync(id);
            if (organization == null)
            {
                return NotFound();
            }

            return Ok(organization);
        }

        /// <summary>
        /// Create a new organization (Admin only)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<OrganizationDto>> CreateOrganization(CreateOrganizationDto createDto)
        {
            // ??? endpoint ??? ???? admins ???
            // ????? ????? ????
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            // Validate input
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _organizationService.CreateAsync(createDto);
                
                _logger.LogInformation("Organization created: {OrganizationName}", result.Name);

                return CreatedAtAction(nameof(GetOrganization), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating organization: {OrganizationName}", createDto.Name);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update organization
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<OrganizationDto>> UpdateOrganization(int id, UpdateOrganizationDto updateDto)
        {
            // ????? ????? ????
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            // ????? ?????? - ??????? ??? ????????? ?????? ?????? ?? ?????? ????
            if (_organizationContext.OrganizationId.HasValue && _organizationContext.OrganizationId.Value != id)
            {
                return Forbid("Access denied to other organizations");
            }

            // ????? ???? ??????
            var existingOrganization = await _organizationService.GetByIdAsync(id);
            if (existingOrganization == null)
            {
                return NotFound();
            }

            // Validate input
            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _organizationService.UpdateAsync(id, updateDto);
                
                _logger.LogInformation("Organization updated: {OrganizationId}", id);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating organization: {OrganizationId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete organization (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrganization(int id)
        {
            // ??? endpoint ??? ???? admins ???
            // ????? ????? ????
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            try
            {
                var result = await _organizationService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                _logger.LogInformation("Organization deleted: {OrganizationId}", id);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting organization: {OrganizationId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get active organizations
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<OrganizationDto>>> GetActiveOrganizations()
        {
            try
            {
                var organizations = await _organizationService.GetActiveOrganizationsAsync();
                
                // ??? ????? ????? ???? ???? ??? ?????? ???? ?? ????? ???
                if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
                {
                    organizations = organizations.Where(o => o.Id == _organizationContext.OrganizationId.Value);
                }

                return Ok(organizations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active organizations");
                return BadRequest("Error retrieving active organizations");
            }
        }

        /// <summary>
        /// Get organization statistics
        /// </summary>
        [HttpGet("{id}/statistics")]
        public async Task<ActionResult> GetOrganizationStatistics(int id)
        {
            // ????? ????? ????
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            // ????? ??????
            if (_organizationContext.OrganizationId.HasValue && _organizationContext.OrganizationId.Value != id)
            {
                return Forbid("Access denied to other organizations");
            }

            try
            {
                var organization = await _organizationService.GetByIdAsync(id);
                if (organization == null)
                {
                    return NotFound();
                }

                // ?? ????? ??????? ???? ???????? ?? ???? ???????? ???????? ???
                var statistics = new
                {
                    OrganizationInfo = new
                    {
                        organization.Id,
                        organization.Name,
                        organization.IsActive,
                        organization.CreatedAt
                    },
                    BranchCount = organization.BranchCount,
                    CategoryCount = organization.CategoryCount,
                    // ?? ????? ??????? ???? ?????? ????? ???:
                    // TicketCount, ActiveAgentCount, CustomerCount, etc.
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving organization statistics: {OrganizationId}", id);
                return BadRequest("Error retrieving statistics");
            }
        }
    }
}