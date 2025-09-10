using Microsoft.AspNetCore.Mvc;
using Helpio.Ir.Application.Services.Core;
using Helpio.Ir.Application.DTOs.Core;
using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.API.Services;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Helpio.Ir.API.Controllers.Core
{
    [ApiController]
    [Route("api/[controller]")]
    public class BranchesController : ControllerBase
    {
        private readonly IBranchService _branchService;
        private readonly IOrganizationContext _organizationContext;
        private readonly IValidator<CreateBranchDto> _createValidator;
        private readonly IValidator<UpdateBranchDto> _updateValidator;
        private readonly ILogger<BranchesController> _logger;

        public BranchesController(
            IBranchService branchService,
            IOrganizationContext organizationContext,
            IValidator<CreateBranchDto> createValidator,
            IValidator<UpdateBranchDto> updateValidator,
            ILogger<BranchesController> logger)
        {
            _branchService = branchService;
            _organizationContext = organizationContext;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        /// <summary>
        /// Get all branches with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<BranchDto>>> GetBranches([FromQuery] PaginationRequest request)
        {
            try
            {
                var result = await _branchService.GetBranchesAsync(request);
                
                // ????? ??????? ?? ???? ?????? ?????
                if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
                {
                    result.Items = result.Items.Where(b => b.OrganizationId == _organizationContext.OrganizationId.Value);
                    result.TotalItems = result.Items.Count();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving branches");
                return BadRequest("Error retrieving branches");
            }
        }

        /// <summary>
        /// Get branch by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<BranchDto>> GetBranch(int id)
        {
            var branch = await _branchService.GetByIdAsync(id);
            if (branch == null)
            {
                return NotFound();
            }

            // ????? ?????? ???????
            if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
            {
                if (branch.OrganizationId != _organizationContext.OrganizationId.Value)
                {
                    return Forbid("Access denied to other organization's branches");
                }
            }

            return Ok(branch);
        }

        /// <summary>
        /// Get branch with teams
        /// </summary>
        [HttpGet("{id}/teams")]
        public async Task<ActionResult<BranchDto>> GetBranchWithTeams(int id)
        {
            var branch = await _branchService.GetWithTeamsAsync(id);
            if (branch == null)
            {
                return NotFound();
            }

            // ????? ?????? ???????
            if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
            {
                if (branch.OrganizationId != _organizationContext.OrganizationId.Value)
                {
                    return Forbid("Access denied to other organization's branches");
                }
            }

            return Ok(branch);
        }

        /// <summary>
        /// Create a new branch
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<BranchDto>> CreateBranch(CreateBranchDto createDto)
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

            // ????? ?????? ???? ????
            createDto.OrganizationId = _organizationContext.OrganizationId.Value;

            // Validate input
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _branchService.CreateAsync(createDto);
                
                _logger.LogInformation("Branch created: {BranchName} for Organization: {OrganizationId}", 
                    result.Name, createDto.OrganizationId);

                return CreatedAtAction(nameof(GetBranch), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating branch: {BranchName}", createDto.Name);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update branch
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<BranchDto>> UpdateBranch(int id, UpdateBranchDto updateDto)
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

            // ????? ???? ???? ? ?????? ???????
            var existingBranch = await _branchService.GetByIdAsync(id);
            if (existingBranch == null)
            {
                return NotFound();
            }

            if (existingBranch.OrganizationId != _organizationContext.OrganizationId.Value)
            {
                return Forbid("Access denied to other organization's branches");
            }

            // Validate input
            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _branchService.UpdateAsync(id, updateDto);
                
                _logger.LogInformation("Branch updated: {BranchId}", id);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating branch: {BranchId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete branch
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBranch(int id)
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

            // ????? ???? ???? ? ?????? ???????
            var existingBranch = await _branchService.GetByIdAsync(id);
            if (existingBranch == null)
            {
                return NotFound();
            }

            if (existingBranch.OrganizationId != _organizationContext.OrganizationId.Value)
            {
                return Forbid("Access denied to other organization's branches");
            }

            try
            {
                var result = await _branchService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                _logger.LogInformation("Branch deleted: {BranchId}", id);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting branch: {BranchId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get branches by organization ID
        /// </summary>
        [HttpGet("organization/{organizationId}")]
        public async Task<ActionResult<IEnumerable<BranchDto>>> GetBranchesByOrganization(int organizationId)
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

            // ????? ?????? ???????
            if (_organizationContext.OrganizationId.Value != organizationId)
            {
                return Forbid("Access denied to other organization's branches");
            }

            try
            {
                var branches = await _branchService.GetByOrganizationIdAsync(organizationId);
                return Ok(branches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving branches for organization: {OrganizationId}", organizationId);
                return BadRequest("Error retrieving branches");
            }
        }

        /// <summary>
        /// Get branches for authenticated organization
        /// </summary>
        [HttpGet("my-organization")]
        public async Task<ActionResult<IEnumerable<BranchDto>>> GetMyOrganizationBranches()
        {
            if (!_organizationContext.OrganizationId.HasValue)
            {
                return BadRequest("Organization context not found");
            }

            try
            {
                var branches = await _branchService.GetByOrganizationIdAsync(_organizationContext.OrganizationId.Value);
                return Ok(branches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving branches for organization: {OrganizationId}", _organizationContext.OrganizationId);
                return BadRequest("Error retrieving branches");
            }
        }

        /// <summary>
        /// Get active branches
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<BranchDto>>> GetActiveBranches()
        {
            try
            {
                var branches = await _branchService.GetActiveBranchesAsync();
                
                // ????? ?? ???? ?????? ??? ????? ????? ???? ???
                if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
                {
                    branches = branches.Where(b => b.OrganizationId == _organizationContext.OrganizationId.Value);
                }

                return Ok(branches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active branches");
                return BadRequest("Error retrieving active branches");
            }
        }
    }
}