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
    public class TicketCategoriesController : ControllerBase
    {
        private readonly ITicketCategoryService _ticketCategoryService;
        private readonly IOrganizationContext _organizationContext;
        private readonly IValidator<CreateTicketCategoryDto> _createValidator;
        private readonly IValidator<UpdateTicketCategoryDto> _updateValidator;
        private readonly ILogger<TicketCategoriesController> _logger;

        public TicketCategoriesController(
            ITicketCategoryService ticketCategoryService,
            IOrganizationContext organizationContext,
            IValidator<CreateTicketCategoryDto> createValidator,
            IValidator<UpdateTicketCategoryDto> updateValidator,
            ILogger<TicketCategoriesController> logger)
        {
            _ticketCategoryService = ticketCategoryService;
            _organizationContext = organizationContext;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        /// <summary>
        /// Get all ticket categories with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<TicketCategoryDto>>> GetTicketCategories([FromQuery] PaginationRequest request)
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
                var result = await _ticketCategoryService.GetCategoriesAsync(request);
                
                // ????? ???????????? ?? ???? ??????
                var filteredCategories = result.Items.Where(c => c.OrganizationId == _organizationContext.OrganizationId.Value);
                result.Items = filteredCategories;
                result.TotalItems = filteredCategories.Count();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ticket categories");
                return BadRequest("Error retrieving ticket categories");
            }
        }

        /// <summary>
        /// Get ticket category by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TicketCategoryDto>> GetTicketCategory(int id)
        {
            var category = await _ticketCategoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // ????? ?????? ???????
            if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
            {
                if (category.OrganizationId != _organizationContext.OrganizationId.Value)
                {
                    return Forbid("Access denied to other organization's categories");
                }
            }

            return Ok(category);
        }

        /// <summary>
        /// Create a new ticket category
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TicketCategoryDto>> CreateTicketCategory(CreateTicketCategoryDto createDto)
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

            // ????? ?????? ???? ?????????
            createDto.OrganizationId = _organizationContext.OrganizationId.Value;

            // Validate input
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _ticketCategoryService.CreateAsync(createDto);
                
                _logger.LogInformation("Ticket category created: {CategoryName} for Organization: {OrganizationId}", 
                    result.Name, createDto.OrganizationId);

                return CreatedAtAction(nameof(GetTicketCategory), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket category: {CategoryName}", createDto.Name);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update ticket category
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<TicketCategoryDto>> UpdateTicketCategory(int id, UpdateTicketCategoryDto updateDto)
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

            // ????? ???? ????????? ? ?????? ???????
            var existingCategory = await _ticketCategoryService.GetByIdAsync(id);
            if (existingCategory == null)
            {
                return NotFound();
            }

            if (existingCategory.OrganizationId != _organizationContext.OrganizationId.Value)
            {
                return Forbid("Access denied to other organization's categories");
            }

            // Validate input
            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _ticketCategoryService.UpdateAsync(id, updateDto);
                
                _logger.LogInformation("Ticket category updated: {CategoryId}", id);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ticket category: {CategoryId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete ticket category
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicketCategory(int id)
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

            // ????? ???? ????????? ? ?????? ???????
            var existingCategory = await _ticketCategoryService.GetByIdAsync(id);
            if (existingCategory == null)
            {
                return NotFound();
            }

            if (existingCategory.OrganizationId != _organizationContext.OrganizationId.Value)
            {
                return Forbid("Access denied to other organization's categories");
            }

            try
            {
                var result = await _ticketCategoryService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                _logger.LogInformation("Ticket category deleted: {CategoryId}", id);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ticket category: {CategoryId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get categories by organization ID
        /// </summary>
        [HttpGet("organization/{organizationId}")]
        public async Task<ActionResult<IEnumerable<TicketCategoryDto>>> GetCategoriesByOrganization(int organizationId)
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
                return Forbid("Access denied to other organization's categories");
            }

            try
            {
                var categories = await _ticketCategoryService.GetByOrganizationIdAsync(organizationId);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories for organization: {OrganizationId}", organizationId);
                return BadRequest("Error retrieving categories");
            }
        }

        /// <summary>
        /// Get categories for authenticated organization
        /// </summary>
        [HttpGet("my-organization")]
        public async Task<ActionResult<IEnumerable<TicketCategoryDto>>> GetMyOrganizationCategories()
        {
            if (!_organizationContext.OrganizationId.HasValue)
            {
                return BadRequest("Organization context not found");
            }

            try
            {
                var categories = await _ticketCategoryService.GetByOrganizationIdAsync(_organizationContext.OrganizationId.Value);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories for organization: {OrganizationId}", _organizationContext.OrganizationId);
                return BadRequest("Error retrieving categories");
            }
        }

        /// <summary>
        /// Get active ticket categories
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<TicketCategoryDto>>> GetActiveCategories()
        {
            try
            {
                var categories = await _ticketCategoryService.GetActiveCategoriesAsync();
                
                // ????? ?? ???? ?????? ??? ????? ????? ???? ???
                if (_organizationContext.IsAuthenticated && _organizationContext.OrganizationId.HasValue)
                {
                    categories = categories.Where(c => c.OrganizationId == _organizationContext.OrganizationId.Value);
                }

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active categories");
                return BadRequest("Error retrieving active categories");
            }
        }
    }
}