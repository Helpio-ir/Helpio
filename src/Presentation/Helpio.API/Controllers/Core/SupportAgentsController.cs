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
    public class SupportAgentsController : ControllerBase
    {
        private readonly ISupportAgentService _supportAgentService;
        private readonly ITeamService _teamService;
        private readonly IOrganizationContext _organizationContext;
        private readonly IValidator<CreateSupportAgentDto> _createValidator;
        private readonly IValidator<UpdateSupportAgentDto> _updateValidator;
        private readonly ILogger<SupportAgentsController> _logger;

        public SupportAgentsController(
            ISupportAgentService supportAgentService,
            ITeamService teamService,
            IOrganizationContext organizationContext,
            IValidator<CreateSupportAgentDto> createValidator,
            IValidator<UpdateSupportAgentDto> updateValidator,
            ILogger<SupportAgentsController> logger)
        {
            _supportAgentService = supportAgentService;
            _teamService = teamService;
            _organizationContext = organizationContext;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        /// <summary>
        /// Get all support agents with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<SupportAgentDto>>> GetSupportAgents([FromQuery] PaginationRequest request)
        {
            // ????? ????? ????
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            try
            {
                var result = await _supportAgentService.GetAgentsAsync(request);
                
                // ????? ????????? ?? ???? ?????? ?????
                if (_organizationContext.OrganizationId.HasValue)
                {
                    result.Items = result.Items.Where(a => HasAgentAccess(a));
                    result.TotalItems = result.Items.Count();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving support agents");
                return BadRequest("Error retrieving support agents");
            }
        }

        /// <summary>
        /// Get support agent by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<SupportAgentDto>> GetSupportAgent(int id)
        {
            var agent = await _supportAgentService.GetByIdAsync(id);
            if (agent == null)
            {
                return NotFound();
            }

            // ????? ?????? ???????
            if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
            {
                if (!HasAgentAccess(agent))
                {
                    return Forbid("Access denied to this support agent");
                }
            }

            return Ok(agent);
        }

        /// <summary>
        /// Get support agent by agent code
        /// </summary>
        [HttpGet("code/{agentCode}")]
        public async Task<ActionResult<SupportAgentDto>> GetSupportAgentByCode(string agentCode)
        {
            var agent = await _supportAgentService.GetByAgentCodeAsync(agentCode);
            if (agent == null)
            {
                return NotFound();
            }

            // ????? ?????? ???????
            if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
            {
                if (!HasAgentAccess(agent))
                {
                    return Forbid("Access denied to this support agent");
                }
            }

            return Ok(agent);
        }

        /// <summary>
        /// Create a new support agent
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<SupportAgentDto>> CreateSupportAgent(CreateSupportAgentDto createDto)
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

            // ????? ?????? ?? Team
            if (!await ValidateTeamAccess(createDto.TeamId))
            {
                return Forbid("Access denied to specified team");
            }

            // Validate input
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _supportAgentService.CreateAsync(createDto);
                
                _logger.LogInformation("Support agent created: {AgentCode} for Team: {TeamId}", 
                    result.AgentCode, createDto.TeamId);

                return CreatedAtAction(nameof(GetSupportAgent), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating support agent: {AgentCode}", createDto.AgentCode);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update support agent
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<SupportAgentDto>> UpdateSupportAgent(int id, UpdateSupportAgentDto updateDto)
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

            // ????? ???? ??????? ? ?????? ???????
            var existingAgent = await _supportAgentService.GetByIdAsync(id);
            if (existingAgent == null)
            {
                return NotFound();
            }

            if (!HasAgentAccess(existingAgent))
            {
                return Forbid("Access denied to this support agent");
            }

            // ????? ?????? ?? Team ????
            if (!await ValidateTeamAccess(updateDto.TeamId))
            {
                return Forbid("Access denied to specified team");
            }

            // Validate input
            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _supportAgentService.UpdateAsync(id, updateDto);
                
                _logger.LogInformation("Support agent updated: {AgentId}", id);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating support agent: {AgentId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete support agent
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupportAgent(int id)
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

            // ????? ???? ??????? ? ?????? ???????
            var existingAgent = await _supportAgentService.GetByIdAsync(id);
            if (existingAgent == null)
            {
                return NotFound();
            }

            if (!HasAgentAccess(existingAgent))
            {
                return Forbid("Access denied to this support agent");
            }

            try
            {
                var result = await _supportAgentService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                _logger.LogInformation("Support agent deleted: {AgentId}", id);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting support agent: {AgentId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get support agents by team ID
        /// </summary>
        [HttpGet("team/{teamId}")]
        public async Task<ActionResult<IEnumerable<SupportAgentDto>>> GetSupportAgentsByTeam(int teamId)
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

            // ????? ?????? ?? Team
            if (!await ValidateTeamAccess(teamId))
            {
                return Forbid("Access denied to this team");
            }

            try
            {
                var agents = await _supportAgentService.GetByTeamIdAsync(teamId);
                return Ok(agents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving support agents for team: {TeamId}", teamId);
                return BadRequest("Error retrieving support agents");
            }
        }

        /// <summary>
        /// Get available support agents
        /// </summary>
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<SupportAgentDto>>> GetAvailableSupportAgents()
        {
            try
            {
                var agents = await _supportAgentService.GetAvailableAgentsAsync();
                
                // ????? ?? ???? ?????? ???????
                if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
                {
                    agents = agents.Where(a => HasAgentAccess(a));
                }

                return Ok(agents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available support agents");
                return BadRequest("Error retrieving available support agents");
            }
        }

        /// <summary>
        /// Get support agents by specialization
        /// </summary>
        [HttpGet("specialization/{specialization}")]
        public async Task<ActionResult<IEnumerable<SupportAgentDto>>> GetSupportAgentsBySpecialization(string specialization)
        {
            try
            {
                var agents = await _supportAgentService.GetBySpecializationAsync(specialization);
                
                // ????? ?? ???? ?????? ???????
                if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
                {
                    agents = agents.Where(a => HasAgentAccess(a));
                }

                return Ok(agents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving support agents by specialization: {Specialization}", specialization);
                return BadRequest("Error retrieving support agents");
            }
        }

        /// <summary>
        /// Get support agents with low workload
        /// </summary>
        [HttpGet("low-workload")]
        public async Task<ActionResult<IEnumerable<SupportAgentDto>>> GetSupportAgentsWithLowWorkload([FromQuery] int maxTickets = 5)
        {
            try
            {
                var agents = await _supportAgentService.GetAgentsWithLowWorkloadAsync(maxTickets);
                
                // ????? ?? ???? ?????? ???????
                if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
                {
                    agents = agents.Where(a => HasAgentAccess(a));
                }

                return Ok(agents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving support agents with low workload");
                return BadRequest("Error retrieving support agents");
            }
        }

        /// <summary>
        /// Set agent availability
        /// </summary>
        [HttpPost("{id}/availability")]
        public async Task<IActionResult> SetAgentAvailability(int id, [FromBody] SetAvailabilityRequest request)
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

            // ????? ???? ??????? ? ?????? ???????
            var existingAgent = await _supportAgentService.GetByIdAsync(id);
            if (existingAgent == null)
            {
                return NotFound();
            }

            if (!HasAgentAccess(existingAgent))
            {
                return Forbid("Access denied to this support agent");
            }

            try
            {
                var result = await _supportAgentService.SetAvailabilityAsync(id, request.IsAvailable);
                if (!result)
                {
                    return BadRequest("Failed to set availability");
                }

                _logger.LogInformation("Agent {AgentId} availability set to {IsAvailable}", id, request.IsAvailable);
                
                return Ok(new { message = "Availability updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting availability for agent: {AgentId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get best available agent for a team
        /// </summary>
        [HttpGet("team/{teamId}/best-available")]
        public async Task<ActionResult<SupportAgentDto>> GetBestAvailableAgent(int teamId, [FromQuery] string? specialization = null)
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

            // ????? ?????? ?? Team
            if (!await ValidateTeamAccess(teamId))
            {
                return Forbid("Access denied to this team");
            }

            try
            {
                var agent = await _supportAgentService.GetBestAvailableAgentAsync(teamId, specialization);
                if (agent == null)
                {
                    return NotFound("No available agent found");
                }

                return Ok(agent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting best available agent for team: {TeamId}", teamId);
                return BadRequest("Error getting best available agent");
            }
        }

        /// <summary>
        /// ????? ?????? ????? ?? ??????? ???????? ?? ???? ??????
        /// </summary>
        private bool HasAgentAccess(SupportAgentDto agent)
        {
            if (!_organizationContext.OrganizationId.HasValue)
                return false;

            // ????? ?????? ?? ???? Team -> Branch -> Organization
            return agent.Team?.Branch?.OrganizationId == _organizationContext.OrganizationId.Value;
        }

        /// <summary>
        /// ????? ?????? ?? Team
        /// </summary>
        private async Task<bool> ValidateTeamAccess(int teamId)
        {
            if (!_organizationContext.OrganizationId.HasValue)
                return false;

            var team = await _teamService.GetByIdAsync(teamId);
            return team?.Branch?.OrganizationId == _organizationContext.OrganizationId.Value;
        }
    }

    // Helper class for request model
    public class SetAvailabilityRequest
    {
        public bool IsAvailable { get; set; }
    }
}