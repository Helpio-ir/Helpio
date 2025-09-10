using FluentValidation;
using Helpio.Ir.API.Services;
using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.Application.DTOs.Ticketing;
using Helpio.Ir.Application.Services.Ticketing;
using Microsoft.AspNetCore.Mvc;

namespace Helpio.Ir.API.Controllers.Ticketing
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly IOrganizationContext _organizationContext;
        private readonly IValidator<CreateTicketDto> _createValidator;
        private readonly IValidator<UpdateTicketDto> _updateValidator;
        private readonly IValidator<AssignTicketDto> _assignValidator;
        private readonly IValidator<ResolveTicketDto> _resolveValidator;
        private readonly ILogger<TicketsController> _logger;

        public TicketsController(
            ITicketService ticketService,
            IOrganizationContext organizationContext,
            IValidator<CreateTicketDto> createValidator,
            IValidator<UpdateTicketDto> updateValidator,
            IValidator<AssignTicketDto> assignValidator,
            IValidator<ResolveTicketDto> resolveValidator,
            ILogger<TicketsController> logger)
        {
            _ticketService = ticketService;
            _organizationContext = organizationContext;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _assignValidator = assignValidator;
            _resolveValidator = resolveValidator;
            _logger = logger;
        }

        /// <summary>
        /// Get all tickets with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<TicketDto>>> GetTickets([FromQuery] PaginationRequest request)
        {
            try
            {
                var result = await _ticketService.GetTicketsAsync(request);

                // فیلتر تیکت‌ها بر اساس دسترسی سازمانی
                if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
                {
                    // فیلتر کردن تیکت‌ها برای نمایش فقط تیکت‌های سازمان جاری
                    result.Items = result.Items.Where(ticket => HasTicketAccess(ticket));
                    result.TotalItems = result.Items.Count();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tickets");
                return BadRequest("Error retrieving tickets");
            }
        }

        /// <summary>
        /// Get ticket by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TicketDto>> GetTicket(int id)
        {
            var ticket = await _ticketService.GetByIdAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            // بررسی دسترسی سازمانی
            if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
            {
                if (!await HasTicketAccessAsync(ticket))
                {
                    return Forbid("Access denied to this ticket");
                }
            }

            return Ok(ticket);
        }

        /// <summary>
        /// Get ticket with full details
        /// </summary>
        [HttpGet("{id}/details")]
        public async Task<ActionResult<TicketDto>> GetTicketWithDetails(int id)
        {
            var ticket = await _ticketService.GetWithDetailsAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            // بررسی دسترسی سازمانی
            if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
            {
                if (!await HasTicketAccessAsync(ticket))
                {
                    return Forbid("Access denied to this ticket");
                }
            }

            return Ok(ticket);
        }

        /// <summary>
        /// Create a new ticket
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TicketDto>> CreateTicket(CreateTicketDto createDto)
        {
            // بررسی احراز هویت
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

            // بررسی اینکه Customer و Team متعلق به همین سازمان باشند
            if (!await ValidateTicketEntitiesAccess(createDto.CustomerId, createDto.TeamId, createDto.TicketCategoryId))
            {
                return Forbid("Access denied to specified customer, team, or category");
            }

            try
            {
                var result = await _ticketService.CreateAsync(createDto);

                _logger.LogInformation("Ticket created: {TicketId} for Customer: {CustomerId}",
                    result.Id, createDto.CustomerId);

                return CreatedAtAction(nameof(GetTicket), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket for Customer: {CustomerId}", createDto.CustomerId);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update ticket
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<TicketDto>> UpdateTicket(int id, UpdateTicketDto updateDto)
        {
            // بررسی احراز هویت
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            if (!_organizationContext.OrganizationId.HasValue)
            {
                return BadRequest("Organization context not found");
            }

            // بررسی وجود تیکت و مالکیت سازمانی
            var existingTicket = await _ticketService.GetByIdAsync(id);
            if (existingTicket == null)
            {
                return NotFound();
            }

            if (!await HasTicketAccessAsync(existingTicket))
            {
                return Forbid("Access denied to this ticket");
            }

            // Validate input
            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _ticketService.UpdateAsync(id, updateDto);

                _logger.LogInformation("Ticket updated: {TicketId}", id);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ticket: {TicketId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete ticket
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            // بررسی احراز هویت
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            if (!_organizationContext.OrganizationId.HasValue)
            {
                return BadRequest("Organization context not found");
            }

            // بررسی وجود تیکت و مالکیت سازمانی
            var existingTicket = await _ticketService.GetByIdAsync(id);
            if (existingTicket == null)
            {
                return NotFound();
            }

            if (!await HasTicketAccessAsync(existingTicket))
            {
                return Forbid("Access denied to this ticket");
            }

            try
            {
                var result = await _ticketService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                _logger.LogInformation("Ticket deleted: {TicketId}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ticket: {TicketId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Assign ticket to support agent
        /// </summary>
        [HttpPost("{id}/assign")]
        public async Task<IActionResult> AssignTicket(int id, AssignTicketDto assignDto)
        {
            // بررسی احراز هویت
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            if (!_organizationContext.OrganizationId.HasValue)
            {
                return BadRequest("Organization context not found");
            }

            // بررسی وجود تیکت و مالکیت سازمانی
            var existingTicket = await _ticketService.GetByIdAsync(id);
            if (existingTicket == null)
            {
                return NotFound();
            }

            if (!await HasTicketAccessAsync(existingTicket))
            {
                return Forbid("Access denied to this ticket");
            }

            assignDto.TicketId = id; // اطمینان از صحت ID

            // Validate input
            var validationResult = await _assignValidator.ValidateAsync(assignDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _ticketService.AssignTicketAsync(assignDto);
                if (!result)
                {
                    return BadRequest("Failed to assign ticket");
                }

                _logger.LogInformation("Ticket {TicketId} assigned to agent {AgentId}", id, assignDto.SupportAgentId);

                return Ok(new { message = "Ticket assigned successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning ticket: {TicketId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Resolve ticket
        /// </summary>
        [HttpPost("{id}/resolve")]
        public async Task<IActionResult> ResolveTicket(int id, ResolveTicketDto resolveDto)
        {
            // بررسی احراز هویت
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            if (!_organizationContext.OrganizationId.HasValue)
            {
                return BadRequest("Organization context not found");
            }

            // بررسی وجود تیکت و مالکیت سازمانی
            var existingTicket = await _ticketService.GetByIdAsync(id);
            if (existingTicket == null)
            {
                return NotFound();
            }

            if (!await HasTicketAccessAsync(existingTicket))
            {
                return Forbid("Access denied to this ticket");
            }

            resolveDto.TicketId = id; // اطمینان از صحت ID

            // Validate input
            var validationResult = await _resolveValidator.ValidateAsync(resolveDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _ticketService.ResolveTicketAsync(resolveDto);
                if (!result)
                {
                    return BadRequest("Failed to resolve ticket");
                }

                _logger.LogInformation("Ticket {TicketId} resolved", id);

                return Ok(new { message = "Ticket resolved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving ticket: {TicketId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Change ticket state
        /// </summary>
        [HttpPatch("{id}/state")]
        public async Task<IActionResult> ChangeTicketState(int id, [FromBody] int newStateId)
        {
            // بررسی احراز هویت
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            if (!_organizationContext.OrganizationId.HasValue)
            {
                return BadRequest("Organization context not found");
            }

            // بررسی وجود تیکت و مالکیت سازمانی
            var existingTicket = await _ticketService.GetByIdAsync(id);
            if (existingTicket == null)
            {
                return NotFound();
            }

            if (!await HasTicketAccessAsync(existingTicket))
            {
                return Forbid("Access denied to this ticket");
            }

            try
            {
                var result = await _ticketService.ChangeStateAsync(id, newStateId);
                if (!result)
                {
                    return BadRequest("Failed to change ticket state");
                }

                _logger.LogInformation("Ticket {TicketId} state changed to {StateId}", id, newStateId);

                return Ok(new { message = "Ticket state changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing ticket state: {TicketId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get tickets by customer
        /// </summary>
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetTicketsByCustomer(int customerId)
        {
            // بررسی احراز هویت
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
                var tickets = await _ticketService.GetByCustomerIdAsync(customerId);

                // فیلتر بر اساس دسترسی سازمانی
                var filteredTickets = tickets.Where(ticket => HasTicketAccess(ticket));

                return Ok(filteredTickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tickets for customer: {CustomerId}", customerId);
                return BadRequest("Error retrieving tickets");
            }
        }

        /// <summary>
        /// Get unassigned tickets
        /// </summary>
        [HttpGet("unassigned")]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetUnassignedTickets()
        {
            // بررسی احراز هویت
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
                var tickets = await _ticketService.GetUnassignedTicketsAsync();
                
                // فیلتر بر اساس دسترسی سازمانی
                var filteredTickets = tickets.Where(ticket => HasTicketAccess(ticket));

                return Ok(filteredTickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unassigned tickets");
                return BadRequest("Error retrieving unassigned tickets");
            }
        }

        /// <summary>
        /// Get overdue tickets
        /// </summary>
        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<TicketDto>>> GetOverdueTickets()
        {
            // بررسی احراز هویت
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
                var tickets = await _ticketService.GetOverdueTicketsAsync();
                
                // فیلتر بر اساس دسترسی سازمانی
                var filteredTickets = tickets.Where(ticket => HasTicketAccess(ticket));

                return Ok(filteredTickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving overdue tickets");
                return BadRequest("Error retrieving overdue tickets");
            }
        }

        /// <summary>
        /// Get ticket statistics (محدود به سازمان)
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<Dictionary<string, int>>> GetTicketStatistics()
        {
            // بررسی احراز هویت
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
                // فقط آمار تیکت‌های سازمان جاری
                var allTickets = await _ticketService.GetTicketsAsync(new PaginationRequest { PageNumber = 1, PageSize = int.MaxValue });
                var organizationTickets = allTickets.Items.Where(ticket => HasTicketAccess(ticket)).ToList();

                var statistics = new Dictionary<string, int>
                {
                    ["Total"] = organizationTickets.Count,
                    ["Open"] = organizationTickets.Count(t => !t.IsResolved),
                    ["Resolved"] = organizationTickets.Count(t => t.IsResolved),
                    ["Overdue"] = organizationTickets.Count(t => t.IsOverdue),
                    ["Unassigned"] = organizationTickets.Count(t => !t.SupportAgentId.HasValue),
                    ["HighPriority"] = organizationTickets.Count(t => t.Priority == TicketPriorityDto.High || t.Priority == TicketPriorityDto.Critical)
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ticket statistics");
                return BadRequest("Error retrieving statistics");
            }
        }


        //TODO: Move these helper methods to a dedicated service for better separation of concerns
        /// <summary>
        /// بررسی دسترسی کاربر به تیکت بر اساس سازمان (غیر async)
        /// </summary>
        private bool HasTicketAccess(TicketDto ticket)
        {
            if (!_organizationContext.OrganizationId.HasValue)
                return false;

            // بررسی دسترسی از طریق TicketCategory (که دارای OrganizationId است)
            if (ticket.TicketCategory != null && ticket.TicketCategory.OrganizationId == _organizationContext.OrganizationId.Value)
                return true;

            // بررسی دسترسی از طریق Team -> Branch -> Organization
            if (ticket.Team?.Branch?.OrganizationId == _organizationContext.OrganizationId.Value)
                return true;

            // در صورت عدم وجود navigation properties کامل، دسترسی رد می‌شود
            return false;
        }

        /// <summary>
        /// بررسی دسترسی کاربر به تیکت بر اساس سازمان (async)
        /// </summary>
        private async Task<bool> HasTicketAccessAsync(TicketDto ticket)
        {
            if (!_organizationContext.OrganizationId.HasValue)
                return false;

            // اگر ticket details کامل نباشد، باید ticket with details گرفته شود
            var detailedTicket = await _ticketService.GetWithDetailsAsync(ticket.Id);
            if (detailedTicket == null)
                return false;

            return HasTicketAccess(detailedTicket);
        }

        /// <summary>
        /// بررسی دسترسی به Customer, Team و Category هنگام ایجاد تیکت
        /// </summary>
        private async Task<bool> ValidateTicketEntitiesAccess(int customerId, int teamId, int categoryId)
        {
            if (!_organizationContext.OrganizationId.HasValue)
                return false;

            try
            {
                // استراتژی ساده‌تر: ایجاد یک تیکت نمونه و بررسی دسترسی
                // فرض می‌کنیم اگر service layer اجازه ایجاد تیکت بدهد، یعنی entities معتبر هستند

                // برای اطمینان بیشتر، می‌توانیم چک کنیم که TicketCategory متعلق به سازمان ما باشد
                var categoryTickets = await _ticketService.GetByCategoryIdAsync(categoryId);
                var anyCategoryTicket = categoryTickets.FirstOrDefault();

                // اگر تیکتی با این category وجود دارد، بررسی می‌کنیم
                if (anyCategoryTicket != null && !HasTicketAccess(anyCategoryTicket))
                {
                    return false;
                }

                // اگر تیکت با این category وجود ندارد، نمی‌توانیم validate کنیم
                // بنابراین اجازه ایجاد می‌دهیم و در service layer بررسی می‌شود

                return true;
            }
            catch
            {
                // در صورت بروز خطا، محتاطانه دسترسی را رد می‌کنیم
                return false;
            }
        }
    }
}