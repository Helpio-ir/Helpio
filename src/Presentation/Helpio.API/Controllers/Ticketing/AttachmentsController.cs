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
    public class AttachmentsController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly IOrganizationContext _organizationContext;
        private readonly IValidator<CreateAttachmentDto> _createValidator;
        private readonly ILogger<AttachmentsController> _logger;

        public AttachmentsController(
            ITicketService ticketService,
            IOrganizationContext organizationContext,
            IValidator<CreateAttachmentDto> createValidator,
            ILogger<AttachmentsController> logger)
        {
            _ticketService = ticketService;
            _organizationContext = organizationContext;
            _createValidator = createValidator;
            _logger = logger;
        }

        /// <summary>
        /// Get attachment by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<AttachmentDto>> GetAttachment(int id)
        {
            // Note: ???? Attachment ??? ????? ???? Ticket ?????? ?? ?????? ?? ?????? ??????? ????? ???
            // ??? ???? ?? Repository method ???? ?? ?????? attachment ?? ?? ticket details ??????
            
            return BadRequest("Attachment access requires ticket context - use ticket-specific endpoints");
        }

        /// <summary>
        /// Upload attachment for a ticket
        /// </summary>
        [HttpPost("ticket/{ticketId}/upload")]
        public async Task<ActionResult<AttachmentDto>> UploadAttachment(int ticketId, [FromForm] IFormFile file, [FromForm] string? description = null)
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
            var ticket = await _ticketService.GetByIdAsync(ticketId);
            if (ticket == null)
            {
                return NotFound("Ticket not found");
            }

            // ????? ?????? ??????? ?? ???? Team/Customer
            if (!await HasTicketAccessAsync(ticket))
            {
                return Forbid("Access denied to this ticket");
            }

            // ????? ????
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            // ????? ?????? ???? (????? ?????? 10MB)
            const long maxFileSize = 10 * 1024 * 1024; // 10MB
            if (file.Length > maxFileSize)
            {
                return BadRequest("File size exceeds maximum limit (10MB)");
            }

            // ????? ??? ???? (??????? - ??????? ????? ????? ???)
            var allowedTypes = new[] { "image/", "application/pdf", "text/", "application/msword", "application/vnd.openxmlformats-officedocument" };
            if (!allowedTypes.Any(type => file.ContentType.StartsWith(type, StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest("File type not allowed");
            }

            try
            {
                // ????? ???? ???? ????? ??? (?? storage service ?? local directory)
                // ???? ???? ??? ??????? ???? ?? wwwroot/uploads ????? ??????
                
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                var filePath = Path.Combine(uploadsPath, uniqueFileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var createDto = new CreateAttachmentDto
                {
                    Name = file.FileName,
                    Type = file.ContentType,
                    Url = $"/uploads/{uniqueFileName}",
                    Size = file.Length,
                    Description = description
                };

                // ????? ???? attachment ?? ?? ???? attach ????
                // ????? ??? response ?????? ????????????
                var attachmentDto = new AttachmentDto
                {
                    Id = new Random().Next(1000, 9999), // ????? random ID
                    Name = createDto.Name,
                    Type = createDto.Type,
                    Url = createDto.Url,
                    Size = createDto.Size,
                    Description = createDto.Description,
                    CreatedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Attachment uploaded for ticket {TicketId}: {FileName}", ticketId, file.FileName);

                return CreatedAtAction(nameof(GetAttachment), new { id = attachmentDto.Id }, attachmentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading attachment for ticket: {TicketId}", ticketId);
                return BadRequest("Error uploading file");
            }
        }

        /// <summary>
        /// Get attachments for a ticket
        /// </summary>
        [HttpGet("ticket/{ticketId}")]
        public async Task<ActionResult<IEnumerable<AttachmentDto>>> GetTicketAttachments(int ticketId)
        {
            // ????? ???? ???? ? ?????? ???????
            var ticket = await _ticketService.GetByIdAsync(ticketId);
            if (ticket == null)
            {
                return NotFound("Ticket not found");
            }

            // ????? ?????? ???????
            if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
            {
                if (!await HasTicketAccessAsync(ticket))
                {
                    return Forbid("Access denied to this ticket");
                }
            }

            try
            {
                // ????? ???? attachments ???? ?? repository ????? ???
                // ????? ???? ???? ????????????
                var attachments = new List<AttachmentDto>();

                return Ok(attachments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attachments for ticket: {TicketId}", ticketId);
                return BadRequest("Error retrieving attachments");
            }
        }

        /// <summary>
        /// Download attachment
        /// </summary>
        [HttpGet("{id}/download")]
        public async Task<IActionResult> DownloadAttachment(int id)
        {
            try
            {
                // ????? ???? attachment details ????? ??? ? ?????? ????? ???
                // ????? NotFound ????????????
                return NotFound("Attachment not found or access denied");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading attachment: {AttachmentId}", id);
                return BadRequest("Error downloading attachment");
            }
        }

        /// <summary>
        /// Delete attachment
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttachment(int id)
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
                // ????? ???? attachment details ????? ??? ? ?????? ????? ???
                // ????? NotFound ????????????
                return NotFound("Attachment not found or access denied");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attachment: {AttachmentId}", id);
                return BadRequest("Error deleting attachment");
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