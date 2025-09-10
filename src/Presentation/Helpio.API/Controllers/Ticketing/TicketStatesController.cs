using Microsoft.AspNetCore.Mvc;
using Helpio.Ir.Application.Services.Ticketing;
using Helpio.Ir.Application.DTOs.Ticketing;
using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.API.Services;
using FluentValidation;

namespace Helpio.Ir.API.Controllers.Ticketing
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketStatesController : ControllerBase
    {
        private readonly ITicketStateService _ticketStateService;
        private readonly IOrganizationContext _organizationContext;
        private readonly IValidator<CreateTicketStateDto> _createValidator;
        private readonly IValidator<UpdateTicketStateDto> _updateValidator;
        private readonly ILogger<TicketStatesController> _logger;

        public TicketStatesController(
            ITicketStateService ticketStateService,
            IOrganizationContext organizationContext,
            IValidator<CreateTicketStateDto> createValidator,
            IValidator<UpdateTicketStateDto> updateValidator,
            ILogger<TicketStatesController> logger)
        {
            _ticketStateService = ticketStateService;
            _organizationContext = organizationContext;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        /// <summary>
        /// Get all ticket states with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<TicketStateDto>>> GetTicketStates([FromQuery] PaginationRequest request)
        {
            try
            {
                var result = await _ticketStateService.GetStatesAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ticket states");
                return BadRequest("Error retrieving ticket states");
            }
        }

        /// <summary>
        /// Get ticket state by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TicketStateDto>> GetTicketState(int id)
        {
            var state = await _ticketStateService.GetByIdAsync(id);
            if (state == null)
            {
                return NotFound();
            }

            return Ok(state);
        }

        /// <summary>
        /// Create a new ticket state
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TicketStateDto>> CreateTicketState(CreateTicketStateDto createDto)
        {
            // ????? ????? ???? (??? admins ????????? state ????? ????)
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            if (!_organizationContext.OrganizationId.HasValue)
            {
                return BadRequest("Organization context not found");
            }

            // Validate input
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _ticketStateService.CreateAsync(createDto);
                
                _logger.LogInformation("Ticket state created: {StateName}", result.Name);

                return CreatedAtAction(nameof(GetTicketState), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket state: {StateName}", createDto.Name);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update ticket state
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<TicketStateDto>> UpdateTicketState(int id, UpdateTicketStateDto updateDto)
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

            // ????? ???? state
            var existingState = await _ticketStateService.GetByIdAsync(id);
            if (existingState == null)
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
                var result = await _ticketStateService.UpdateAsync(id, updateDto);
                
                _logger.LogInformation("Ticket state updated: {StateId}", id);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ticket state: {StateId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete ticket state
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicketState(int id)
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

            // ????? ???? state
            var existingState = await _ticketStateService.GetByIdAsync(id);
            if (existingState == null)
            {
                return NotFound();
            }

            try
            {
                var result = await _ticketStateService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                _logger.LogInformation("Ticket state deleted: {StateId}", id);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ticket state: {StateId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get initial states
        /// </summary>
        [HttpGet("initial")]
        public async Task<ActionResult<IEnumerable<TicketStateDto>>> GetInitialStates()
        {
            try
            {
                var states = await _ticketStateService.GetOrderedStatesAsync();
                var initialStates = states.Where(s => s.Order <= 1);
                return Ok(initialStates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving initial states");
                return BadRequest("Error retrieving initial states");
            }
        }

        /// <summary>
        /// Get final states
        /// </summary>
        [HttpGet("final")]
        public async Task<ActionResult<IEnumerable<TicketStateDto>>> GetFinalStates()
        {
            try
            {
                var states = await _ticketStateService.GetFinalStatesAsync();
                return Ok(states);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving final states");
                return BadRequest("Error retrieving final states");
            }
        }

        /// <summary>
        /// Get default state
        /// </summary>
        [HttpGet("default")]
        public async Task<ActionResult<TicketStateDto>> GetDefaultState()
        {
            try
            {
                var state = await _ticketStateService.GetDefaultStateAsync();
                if (state == null)
                {
                    return NotFound("No default state found");
                }

                return Ok(state);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving default state");
                return BadRequest("Error retrieving default state");
            }
        }

        /// <summary>
        /// Get active states
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<TicketStateDto>>> GetActiveStates()
        {
            try
            {
                // ??? state ??? non-final ?? ???????????? (???? active)
                var states = await _ticketStateService.GetOrderedStatesAsync();
                var activeStates = states.Where(s => !s.IsFinal);
                return Ok(activeStates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active states");
                return BadRequest("Error retrieving active states");
            }
        }

        /// <summary>
        /// Set state as default
        /// </summary>
        [HttpPost("{id}/set-default")]
        public async Task<IActionResult> SetDefaultState(int id)
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

            try
            {
                var state = await _ticketStateService.GetByIdAsync(id);
                if (state == null)
                {
                    return NotFound("State not found");
                }

                var updateDto = new UpdateTicketStateDto
                {
                    Name = state.Name,
                    Description = state.Description,
                    Order = state.Order,
                    ColorCode = state.ColorCode,
                    IsFinal = state.IsFinal,
                    IsDefault = true
                };

                await _ticketStateService.UpdateAsync(id, updateDto);

                _logger.LogInformation("State {StateId} set as default", id);
                
                return Ok(new { message = "State set as default successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting default state: {StateId}", id);
                return BadRequest(ex.Message);
            }
        }
    }
}