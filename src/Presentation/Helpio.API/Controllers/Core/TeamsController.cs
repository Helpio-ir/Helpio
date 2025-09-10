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
    public class TeamsController : ControllerBase
    {
        private readonly ITeamService _teamService;
        private readonly IBranchService _branchService;
        private readonly IOrganizationContext _organizationContext;
        private readonly IValidator<CreateTeamDto> _createValidator;
        private readonly IValidator<UpdateTeamDto> _updateValidator;
        private readonly ILogger<TeamsController> _logger;

        public TeamsController(
            ITeamService teamService,
            IBranchService branchService,
            IOrganizationContext organizationContext,
            IValidator<CreateTeamDto> createValidator,
            IValidator<UpdateTeamDto> updateValidator,
            ILogger<TeamsController> logger)
        {
            _teamService = teamService;
            _branchService = branchService;
            _organizationContext = organizationContext;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        /// <summary>
        /// Get all teams with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<TeamDto>>> GetTeams([FromQuery] PaginationRequest request)
        {
            try
            {
                var result = await _teamService.GetTeamsAsync(request);
                
                // ????? ?????? ?? ???? ?????? ?????
                if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
                {
                    result.Items = result.Items.Where(t => HasTeamAccess(t));
                    result.TotalItems = result.Items.Count();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving teams");
                return BadRequest("Error retrieving teams");
            }
        }

        /// <summary>
        /// Get team by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TeamDto>> GetTeam(int id)
        {
            var team = await _teamService.GetByIdAsync(id);
            if (team == null)
            {
                return NotFound();
            }

            // ????? ?????? ???????
            if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
            {
                if (!HasTeamAccess(team))
                {
                    return Forbid("Access denied to this team");
                }
            }

            return Ok(team);
        }

        /// <summary>
        /// Get team with support agents
        /// </summary>
        [HttpGet("{id}/agents")]
        public async Task<ActionResult<TeamDto>> GetTeamWithAgents(int id)
        {
            var team = await _teamService.GetWithSupportAgentsAsync(id);
            if (team == null)
            {
                return NotFound();
            }

            // ????? ?????? ???????
            if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
            {
                if (!HasTeamAccess(team))
                {
                    return Forbid("Access denied to this team");
                }
            }

            return Ok(team);
        }

        /// <summary>
        /// Create a new team
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TeamDto>> CreateTeam(CreateTeamDto createDto)
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

            // ????? ?????? ?? Branch
            if (!await ValidateBranchAccess(createDto.BranchId))
            {
                return Forbid("Access denied to specified branch");
            }

            // Validate input
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _teamService.CreateAsync(createDto);
                
                _logger.LogInformation("Team created: {TeamName} for Branch: {BranchId}", 
                    result.Name, createDto.BranchId);

                return CreatedAtAction(nameof(GetTeam), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating team: {TeamName}", createDto.Name);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update team
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<TeamDto>> UpdateTeam(int id, UpdateTeamDto updateDto)
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

            // ????? ???? ??? ? ?????? ???????
            var existingTeam = await _teamService.GetByIdAsync(id);
            if (existingTeam == null)
            {
                return NotFound();
            }

            if (!HasTeamAccess(existingTeam))
            {
                return Forbid("Access denied to this team");
            }

            // Validate input
            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _teamService.UpdateAsync(id, updateDto);
                
                _logger.LogInformation("Team updated: {TeamId}", id);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating team: {TeamId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete team
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeam(int id)
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

            // ????? ???? ??? ? ?????? ???????
            var existingTeam = await _teamService.GetByIdAsync(id);
            if (existingTeam == null)
            {
                return NotFound();
            }

            if (!HasTeamAccess(existingTeam))
            {
                return Forbid("Access denied to this team");
            }

            try
            {
                var result = await _teamService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                _logger.LogInformation("Team deleted: {TeamId}", id);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting team: {TeamId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get teams by branch ID
        /// </summary>
        [HttpGet("branch/{branchId}")]
        public async Task<ActionResult<IEnumerable<TeamDto>>> GetTeamsByBranch(int branchId)
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

            // ????? ?????? ?? Branch
            if (!await ValidateBranchAccess(branchId))
            {
                return Forbid("Access denied to this branch");
            }

            try
            {
                var teams = await _teamService.GetByBranchIdAsync(branchId);
                return Ok(teams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving teams for branch: {BranchId}", branchId);
                return BadRequest("Error retrieving teams");
            }
        }

        /// <summary>
        /// Get active teams
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<TeamDto>>> GetActiveTeams()
        {
            try
            {
                var teams = await _teamService.GetActiveTeamsAsync();
                
                // ????? ?? ???? ?????? ??? ????? ????? ???? ???
                if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
                {
                    teams = teams.Where(t => HasTeamAccess(t));
                }

                return Ok(teams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active teams");
                return BadRequest("Error retrieving active teams");
            }
        }

        /// <summary>
        /// Get teams by manager
        /// </summary>
        [HttpGet("manager/{managerId}")]
        public async Task<ActionResult<IEnumerable<TeamDto>>> GetTeamsByManager(int managerId)
        {
            // ????? ????? ????
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            try
            {
                var teams = await _teamService.GetTeamsByManagerAsync(managerId);
                
                // ????? ?? ???? ?????? ???????
                var filteredTeams = teams.Where(t => HasTeamAccess(t));

                return Ok(filteredTeams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving teams for manager: {ManagerId}", managerId);
                return BadRequest("Error retrieving teams");
            }
        }

        /// <summary>
        /// ????? ?????? ????? ?? ??? ?? ???? ??????
        /// </summary>
        private bool HasTeamAccess(TeamDto team)
        {
            if (!_organizationContext.OrganizationId.HasValue)
                return false;

            // ????? ?????? ?? ???? Branch -> Organization
            return team.Branch?.OrganizationId == _organizationContext.OrganizationId.Value;
        }

        /// <summary>
        /// ????? ?????? ?? Branch
        /// </summary>
        private async Task<bool> ValidateBranchAccess(int branchId)
        {
            if (!_organizationContext.OrganizationId.HasValue)
                return false;

            var branch = await _branchService.GetByIdAsync(branchId);
            return branch?.OrganizationId == _organizationContext.OrganizationId.Value;
        }
    }
}