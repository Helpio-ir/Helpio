using Microsoft.AspNetCore.Mvc;
using Helpio.Ir.Application.Services.Ticketing;
using Helpio.Ir.Application.DTOs.Ticketing;
using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.API.Services;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Helpio.Ir.API.Controllers.Ticketing
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResponsesController : ControllerBase
    {
        private readonly IResponseService _responseService;
        private readonly ITicketService _ticketService;
        private readonly IOrganizationContext _organizationContext;
        private readonly IValidator<CreateResponseDto> _createValidator;
        private readonly IValidator<UpdateResponseDto> _updateValidator;
        private readonly ILogger<ResponsesController> _logger;

        public ResponsesController(
            IResponseService responseService,
            ITicketService ticketService,
            IOrganizationContext organizationContext,
            IValidator<CreateResponseDto> createValidator,
            IValidator<UpdateResponseDto> updateValidator,
            ILogger<ResponsesController> logger)
        {
            _responseService = responseService;
            _ticketService = ticketService;
            _organizationContext = organizationContext;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        /// <summary>
        /// Get response by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseDto>> GetResponse(int id)
        {
            var response = await _responseService.GetByIdAsync(id);
            if (response == null)
            {
                return NotFound();
            }

            // ????? ?????? ?? ???? ????
            var ticket = await _ticketService.GetByIdAsync(response.TicketId);
            if (ticket == null)
            {
                return NotFound();
            }

            if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
            {
                if (!await HasTicketAccessAsync(ticket))
                {
                    return Forbid("Access denied to this response");
                }
            }

            return Ok(response);
        }

        /// <summary>
        /// Get responses for a ticket
        /// </summary>
        [HttpGet("ticket/{ticketId}")]
        public async Task<ActionResult<IEnumerable<ResponseDto>>> GetTicketResponses(int ticketId)
        {
            // ????? ???? ???? ? ??????
            var ticket = await _ticketService.GetByIdAsync(ticketId);
            if (ticket == null)
            {
                return NotFound("Ticket not found");
            }

            if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
            {
                if (!await HasTicketAccessAsync(ticket))
                {
                    return Forbid("Access denied to this ticket");
                }
            }

            try
            {
                var responses = await _responseService.GetByTicketIdAsync(ticketId);
                return Ok(responses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving responses for ticket: {TicketId}", ticketId);
                return BadRequest("Error retrieving responses");
            }
        }

        /// <summary>
        /// Get customer responses for a ticket
        /// </summary>
        [HttpGet("ticket/{ticketId}/customer")]
        public async Task<ActionResult<IEnumerable<ResponseDto>>> GetCustomerResponses(int ticketId)
        {
            // ????? ???? ???? ? ??????
            var ticket = await _ticketService.GetByIdAsync(ticketId);
            if (ticket == null)
            {
                return NotFound("Ticket not found");
            }

            if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
            {
                if (!await HasTicketAccessAsync(ticket))
                {
                    return Forbid("Access denied to this ticket");
                }
            }

            try
            {
                var responses = await _responseService.GetCustomerResponsesAsync(ticketId);
                return Ok(responses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer responses for ticket: {TicketId}", ticketId);
                return BadRequest("Error retrieving customer responses");
            }
        }

        /// <summary>
        /// Get agent responses for a ticket
        /// </summary>
        [HttpGet("ticket/{ticketId}/agent")]
        public async Task<ActionResult<IEnumerable<ResponseDto>>> GetAgentResponses(int ticketId)
        {
            // ????? ???? ???? ? ??????
            var ticket = await _ticketService.GetByIdAsync(ticketId);
            if (ticket == null)
            {
                return NotFound("Ticket not found");
            }

            if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
            {
                if (!await HasTicketAccessAsync(ticket))
                {
                    return Forbid("Access denied to this ticket");
                }
            }

            try
            {
                var responses = await _responseService.GetAgentResponsesAsync(ticketId);
                return Ok(responses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving agent responses for ticket: {TicketId}", ticketId);
                return BadRequest("Error retrieving agent responses");
            }
        }

        /// <summary>
        /// Get unread responses for a ticket
        /// </summary>
        [HttpGet("ticket/{ticketId}/unread")]
        public async Task<ActionResult<IEnumerable<ResponseDto>>> GetUnreadResponses(int ticketId)
        {
            // ????? ???? ???? ? ??????
            var ticket = await _ticketService.GetByIdAsync(ticketId);
            if (ticket == null)
            {
                return NotFound("Ticket not found");
            }

            if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
            {
                if (!await HasTicketAccessAsync(ticket))
                {
                    return Forbid("Access denied to this ticket");
                }
            }

            try
            {
                var responses = await _responseService.GetUnreadResponsesAsync(ticketId);
                return Ok(responses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unread responses for ticket: {TicketId}", ticketId);
                return BadRequest("Error retrieving unread responses");
            }
        }

        /// <summary>
        /// Get latest response for a ticket
        /// </summary>
        [HttpGet("ticket/{ticketId}/latest")]
        public async Task<ActionResult<ResponseDto>> GetLatestResponse(int ticketId)
        {
            // ????? ???? ???? ? ??????
            var ticket = await _ticketService.GetByIdAsync(ticketId);
            if (ticket == null)
            {
                return NotFound("Ticket not found");
            }

            if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
            {
                if (!await HasTicketAccessAsync(ticket))
                {
                    return Forbid("Access denied to this ticket");
                }
            }

            try
            {
                var response = await _responseService.GetLatestResponseAsync(ticketId);
                if (response == null)
                {
                    return NotFound("No responses found for this ticket");
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving latest response for ticket: {TicketId}", ticketId);
                return BadRequest("Error retrieving latest response");
            }
        }

        /// <summary>
        /// Create a new response
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ResponseDto>> CreateResponse(CreateResponseDto createDto)
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

            // ????? ???? ???? ? ??????
            var ticket = await _ticketService.GetByIdAsync(createDto.TicketId);
            if (ticket == null)
            {
                return NotFound("Ticket not found");
            }

            if (!await HasTicketAccessAsync(ticket))
            {
                return Forbid("Access denied to this ticket");
            }

            // Validate input
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _responseService.CreateAsync(createDto);
                
                _logger.LogInformation("Response created: {ResponseId} for Ticket: {TicketId}", 
                    result.Id, createDto.TicketId);

                return CreatedAtAction(nameof(GetResponse), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating response for ticket: {TicketId}", createDto.TicketId);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update response
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseDto>> UpdateResponse(int id, UpdateResponseDto updateDto)
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

            // ????? ???? response ? ??????
            var existingResponse = await _responseService.GetByIdAsync(id);
            if (existingResponse == null)
            {
                return NotFound();
            }

            // ????? ?????? ?? ???? ????
            var ticket = await _ticketService.GetByIdAsync(existingResponse.TicketId);
            if (ticket == null)
            {
                return NotFound();
            }

            if (!await HasTicketAccessAsync(ticket))
            {
                return Forbid("Access denied to this response");
            }

            // Validate input
            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _responseService.UpdateAsync(id, updateDto);
                
                _logger.LogInformation("Response updated: {ResponseId}", id);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating response: {ResponseId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete response
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResponse(int id)
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

            // ????? ???? response ? ??????
            var existingResponse = await _responseService.GetByIdAsync(id);
            if (existingResponse == null)
            {
                return NotFound();
            }

            // ????? ?????? ?? ???? ????
            var ticket = await _ticketService.GetByIdAsync(existingResponse.TicketId);
            if (ticket == null)
            {
                return NotFound();
            }

            if (!await HasTicketAccessAsync(ticket))
            {
                return Forbid("Access denied to this response");
            }

            try
            {
                var result = await _responseService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                _logger.LogInformation("Response deleted: {ResponseId}", id);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting response: {ResponseId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Mark response as read
        /// </summary>
        [HttpPost("{id}/mark-read")]
        public async Task<IActionResult> MarkResponseAsRead(int id)
        {
            // ????? ????? ????
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            // ????? ???? response ? ??????
            var existingResponse = await _responseService.GetByIdAsync(id);
            if (existingResponse == null)
            {
                return NotFound();
            }

            // ????? ?????? ?? ???? ????
            var ticket = await _ticketService.GetByIdAsync(existingResponse.TicketId);
            if (ticket == null)
            {
                return NotFound();
            }

            if (_organizationContext.OrganizationId.HasValue && !await HasTicketAccessAsync(ticket))
            {
                return Forbid("Access denied to this response");
            }

            try
            {
                var result = await _responseService.MarkAsReadAsync(id);
                if (!result)
                {
                    return BadRequest("Failed to mark response as read");
                }

                _logger.LogInformation("Response {ResponseId} marked as read", id);
                
                return Ok(new { message = "Response marked as read successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking response as read: {ResponseId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// ????? ?????? ????? ?? ???? ?? ???? ??????
        /// </summary>
        private async Task<bool> HasTicketAccessAsync(TicketDto ticket)
        {
            if (!_organizationContext.OrganizationId.HasValue)
                return false;

            // ????? ???? ????? ??? ?? ??? ???? ????? ?? ?????? ????? ??? ?? ??
            // ??? ???? ?? ??????? Team/Customer ???? ?? ?? TicketDto ????
            // ????? true ????????????
            return true;
        }
    }
}