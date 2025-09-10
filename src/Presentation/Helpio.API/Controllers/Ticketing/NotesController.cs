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
    public class NotesController : ControllerBase
    {
        private readonly INoteService _noteService;
        private readonly ITicketService _ticketService;
        private readonly IOrganizationContext _organizationContext;
        private readonly IValidator<CreateNoteDto> _createValidator;
        private readonly IValidator<UpdateNoteDto> _updateValidator;
        private readonly ILogger<NotesController> _logger;

        public NotesController(
            INoteService noteService,
            ITicketService ticketService,
            IOrganizationContext organizationContext,
            IValidator<CreateNoteDto> createValidator,
            IValidator<UpdateNoteDto> updateValidator,
            ILogger<NotesController> logger)
        {
            _noteService = noteService;
            _ticketService = ticketService;
            _organizationContext = organizationContext;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        /// <summary>
        /// Get note by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<NoteDto>> GetNote(int id)
        {
            var note = await _noteService.GetByIdAsync(id);
            if (note == null)
            {
                return NotFound();
            }

            // ????? ?????? ?? ???? ????
            var ticket = await _ticketService.GetByIdAsync(note.TicketId);
            if (ticket == null)
            {
                return NotFound();
            }

            if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
            {
                if (!await HasTicketAccessAsync(ticket))
                {
                    return Forbid("Access denied to this note");
                }
            }

            return Ok(note);
        }

        /// <summary>
        /// Get notes for a ticket
        /// </summary>
        [HttpGet("ticket/{ticketId}")]
        public async Task<ActionResult<IEnumerable<NoteDto>>> GetTicketNotes(int ticketId)
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
                var notes = await _noteService.GetByTicketIdAsync(ticketId);
                return Ok(notes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notes for ticket: {TicketId}", ticketId);
                return BadRequest("Error retrieving notes");
            }
        }

        /// <summary>
        /// Get public notes for a ticket
        /// </summary>
        [HttpGet("ticket/{ticketId}/public")]
        public async Task<ActionResult<IEnumerable<NoteDto>>> GetPublicNotes(int ticketId)
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
                var notes = await _noteService.GetPublicNotesAsync(ticketId);
                return Ok(notes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving public notes for ticket: {TicketId}", ticketId);
                return BadRequest("Error retrieving public notes");
            }
        }

        /// <summary>
        /// Get private notes for a ticket (agents only)
        /// </summary>
        [HttpGet("ticket/{ticketId}/private")]
        public async Task<ActionResult<IEnumerable<NoteDto>>> GetPrivateNotes(int ticketId)
        {
            // ????? ????? ???? (??????????? ????? ??? ???? agents)
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            if (!_organizationContext.OrganizationId.HasValue)
            {
                return BadRequest("Organization context not found");
            }

            // ????? ???? ???? ? ??????
            var ticket = await _ticketService.GetByIdAsync(ticketId);
            if (ticket == null)
            {
                return NotFound("Ticket not found");
            }

            if (!await HasTicketAccessAsync(ticket))
            {
                return Forbid("Access denied to this ticket");
            }

            try
            {
                var notes = await _noteService.GetPrivateNotesAsync(ticketId);
                return Ok(notes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving private notes for ticket: {TicketId}", ticketId);
                return BadRequest("Error retrieving private notes");
            }
        }

        /// <summary>
        /// Get system notes for a ticket
        /// </summary>
        [HttpGet("ticket/{ticketId}/system")]
        public async Task<ActionResult<IEnumerable<NoteDto>>> GetSystemNotes(int ticketId)
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
                var notes = await _noteService.GetSystemNotesAsync(ticketId);
                return Ok(notes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system notes for ticket: {TicketId}", ticketId);
                return BadRequest("Error retrieving system notes");
            }
        }

        /// <summary>
        /// Create a new note
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<NoteDto>> CreateNote(CreateNoteDto createDto)
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
                var result = await _noteService.CreateAsync(createDto);
                
                _logger.LogInformation("Note created: {NoteId} for Ticket: {TicketId}", 
                    result.Id, createDto.TicketId);

                return CreatedAtAction(nameof(GetNote), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating note for ticket: {TicketId}", createDto.TicketId);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update note
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<NoteDto>> UpdateNote(int id, UpdateNoteDto updateDto)
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

            // ????? ???? note ? ??????
            var existingNote = await _noteService.GetByIdAsync(id);
            if (existingNote == null)
            {
                return NotFound();
            }

            // ????? ?????? ???? ?????? ??????
            if (existingNote.IsSystemNote)
            {
                return BadRequest("System notes cannot be edited");
            }

            // ????? ?????? ?? ???? ????
            var ticket = await _ticketService.GetByIdAsync(existingNote.TicketId);
            if (ticket == null)
            {
                return NotFound();
            }

            if (!await HasTicketAccessAsync(ticket))
            {
                return Forbid("Access denied to this note");
            }

            // Validate input
            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _noteService.UpdateAsync(id, updateDto);
                
                _logger.LogInformation("Note updated: {NoteId}", id);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating note: {NoteId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete note
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(int id)
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

            // ????? ???? note ? ??????
            var existingNote = await _noteService.GetByIdAsync(id);
            if (existingNote == null)
            {
                return NotFound();
            }

            // ????? ?????? ???? ??? ??????
            if (existingNote.IsSystemNote)
            {
                return BadRequest("System notes cannot be deleted");
            }

            // ????? ?????? ?? ???? ????
            var ticket = await _ticketService.GetByIdAsync(existingNote.TicketId);
            if (ticket == null)
            {
                return NotFound();
            }

            if (!await HasTicketAccessAsync(ticket))
            {
                return Forbid("Access denied to this note");
            }

            try
            {
                var result = await _noteService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                _logger.LogInformation("Note deleted: {NoteId}", id);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting note: {NoteId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Create system note
        /// </summary>
        [HttpPost("ticket/{ticketId}/system")]
        public async Task<ActionResult<NoteDto>> CreateSystemNote(int ticketId, [FromBody] string description)
        {
            // ????? ????? ???? (??? agents ????????? ????? ??? ????? ????)
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            if (!_organizationContext.OrganizationId.HasValue)
            {
                return BadRequest("Organization context not found");
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                return BadRequest("Description is required");
            }

            // ????? ???? ???? ? ??????
            var ticket = await _ticketService.GetByIdAsync(ticketId);
            if (ticket == null)
            {
                return NotFound("Ticket not found");
            }

            if (!await HasTicketAccessAsync(ticket))
            {
                return Forbid("Access denied to this ticket");
            }

            try
            {
                var result = await _noteService.CreateSystemNoteAsync(ticketId, description);
                
                _logger.LogInformation("System note created: {NoteId} for Ticket: {TicketId}", 
                    result.Id, ticketId);

                return CreatedAtAction(nameof(GetNote), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating system note for ticket: {TicketId}", ticketId);
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
            // ????? true ????????????
            return true;
        }
    }
}