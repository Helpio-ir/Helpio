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
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IOrganizationContext _organizationContext;
        private readonly IValidator<CreateCustomerDto> _createValidator;
        private readonly IValidator<UpdateCustomerDto> _updateValidator;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(
            ICustomerService customerService,
            IOrganizationContext organizationContext,
            IValidator<CreateCustomerDto> createValidator,
            IValidator<UpdateCustomerDto> updateValidator,
            ILogger<CustomersController> logger)
        {
            _customerService = customerService;
            _organizationContext = organizationContext;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        /// <summary>
        /// Get all customers with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<CustomerDto>>> GetCustomers([FromQuery] PaginationRequest request)
        {
            try
            {
                var result = await _customerService.GetCustomersAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers");
                return BadRequest("Error retrieving customers");
            }
        }

        /// <summary>
        /// Get customer by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer);
        }

        /// <summary>
        /// Get customer by email
        /// </summary>
        [HttpGet("email/{email}")]
        public async Task<ActionResult<CustomerDto>> GetCustomerByEmail(string email)
        {
            var customer = await _customerService.GetByEmailAsync(email);
            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer);
        }

        /// <summary>
        /// Create a new customer
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CustomerDto>> CreateCustomer(CreateCustomerDto createDto)
        {
            // Validate input
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _customerService.CreateAsync(createDto);
                
                _logger.LogInformation("Customer created: {CustomerEmail}", result.Email);

                return CreatedAtAction(nameof(GetCustomer), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer: {CustomerEmail}", createDto.Email);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update customer
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<CustomerDto>> UpdateCustomer(int id, UpdateCustomerDto updateDto)
        {
            // ????? ???? ?????
            var existingCustomer = await _customerService.GetByIdAsync(id);
            if (existingCustomer == null)
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
                var result = await _customerService.UpdateAsync(id, updateDto);
                
                _logger.LogInformation("Customer updated: {CustomerId}", id);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer: {CustomerId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete customer
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            // ????? ????? ???? (??? ????? ???? ?? ???? ????)
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            try
            {
                var result = await _customerService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                _logger.LogInformation("Customer deleted: {CustomerId}", id);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer: {CustomerId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Search customers
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> SearchCustomers([FromQuery] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest("Search term is required");
            }

            try
            {
                var customers = await _customerService.SearchCustomersAsync(searchTerm);
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching customers with term: {SearchTerm}", searchTerm);
                return BadRequest("Error searching customers");
            }
        }

        /// <summary>
        /// Get customers with tickets
        /// </summary>
        [HttpGet("with-tickets")]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomersWithTickets()
        {
            try
            {
                var customers = await _customerService.GetCustomersWithTicketsAsync();
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers with tickets");
                return BadRequest("Error retrieving customers with tickets");
            }
        }

        /// <summary>
        /// Check if email exists
        /// </summary>
        [HttpGet("email-exists/{email}")]
        public async Task<ActionResult<bool>> EmailExists(string email)
        {
            try
            {
                var exists = await _customerService.EmailExistsAsync(email);
                return Ok(new { exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email existence: {Email}", email);
                return BadRequest("Error checking email");
            }
        }

        /// <summary>
        /// Get customer statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult> GetCustomerStatistics()
        {
            try
            {
                // ?????? ??? ??????? ???? ?????? ????
                var allCustomers = await _customerService.GetCustomersAsync(new PaginationRequest 
                { 
                    PageNumber = 1, 
                    PageSize = int.MaxValue 
                });

                var statistics = new
                {
                    TotalCustomers = allCustomers.TotalItems,
                    CustomersWithTickets = (await _customerService.GetCustomersWithTicketsAsync()).Count(),
                    RecentlyRegistered = allCustomers.Items.Count(c => c.CreatedAt > DateTime.UtcNow.AddDays(-30)),
                    TopCompanies = allCustomers.Items
                        .Where(c => !string.IsNullOrEmpty(c.CompanyName))
                        .GroupBy(c => c.CompanyName)
                        .OrderByDescending(g => g.Count())
                        .Take(10)
                        .Select(g => new { Company = g.Key, Count = g.Count() })
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer statistics");
                return BadRequest("Error retrieving statistics");
            }
        }
    }
}