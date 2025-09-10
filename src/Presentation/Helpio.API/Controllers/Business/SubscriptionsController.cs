using Microsoft.AspNetCore.Mvc;
using Helpio.Ir.Application.Services.Business;
using Helpio.Ir.Application.DTOs.Business;
using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.API.Services;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Helpio.Ir.API.Controllers.Business
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly IOrganizationContext _organizationContext;
        private readonly IValidator<CreateSubscriptionDto> _createValidator;
        private readonly IValidator<UpdateSubscriptionDto> _updateValidator;
        private readonly ILogger<SubscriptionsController> _logger;

        public SubscriptionsController(
            ISubscriptionService subscriptionService,
            IOrganizationContext organizationContext,
            IValidator<CreateSubscriptionDto> createValidator,
            IValidator<UpdateSubscriptionDto> updateValidator,
            ILogger<SubscriptionsController> logger)
        {
            _subscriptionService = subscriptionService;
            _organizationContext = organizationContext;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        /// <summary>
        /// Get all subscriptions with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<SubscriptionDto>>> GetSubscriptions([FromQuery] PaginationRequest request)
        {
            try
            {
                var result = await _subscriptionService.GetSubscriptionsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subscriptions");
                return BadRequest("Error retrieving subscriptions");
            }
        }

        /// <summary>
        /// Get subscription by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<SubscriptionDto>> GetSubscription(int id)
        {
            var subscription = await _subscriptionService.GetByIdAsync(id);
            if (subscription == null)
            {
                return NotFound();
            }

            return Ok(subscription);
        }

        /// <summary>
        /// Create a new subscription
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<SubscriptionDto>> CreateSubscription(CreateSubscriptionDto createDto)
        {
            // Validate input
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            // Set organization context if available and not already set
            if (!createDto.OrganizationId.HasValue && _organizationContext.OrganizationId.HasValue)
            {
                createDto.OrganizationId = _organizationContext.OrganizationId.Value;
            }

            try
            {
                var result = await _subscriptionService.CreateAsync(createDto);
                
                _logger.LogInformation("Subscription created: {SubscriptionName} for Organization: {OrganizationId}", 
                    result.Name, createDto.OrganizationId);

                return CreatedAtAction(nameof(GetSubscription), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating subscription: {SubscriptionName}", createDto.Name);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update subscription
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<SubscriptionDto>> UpdateSubscription(int id, UpdateSubscriptionDto updateDto)
        {
            // Validate input
            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _subscriptionService.UpdateAsync(id, updateDto);
                
                _logger.LogInformation("Subscription updated: {SubscriptionId}", id);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating subscription: {SubscriptionId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete subscription
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubscription(int id)
        {
            try
            {
                var result = await _subscriptionService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                _logger.LogInformation("Subscription deleted: {SubscriptionId}", id);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting subscription: {SubscriptionId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get subscriptions by organization ID
        /// </summary>
        [HttpGet("organization/{organizationId}")]
        public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetSubscriptionsByOrganization(int organizationId)
        {
            // ??? ????? ??????? ?? ????? ????? ???? ??? ????
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            // ??? ????? ??????? ?? OrganizationId ????? ????
            if (!_organizationContext.OrganizationId.HasValue)
            {
                return BadRequest("Organization context not found");
            }

            // ?? ????? ????? ??????? ?? ????? ??? ?? ???????? ?????? ???? ?????? ????? ????
            if (_organizationContext.OrganizationId.Value != organizationId)
            {
                return Forbid("Access denied to other organization's subscriptions");
            }

            try
            {
                var subscriptions = await _subscriptionService.GetByOrganizationIdAsync(organizationId);
                return Ok(subscriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subscriptions for organization: {OrganizationId}", organizationId);
                return BadRequest("Error retrieving subscriptions");
            }
        }

        /// <summary>
        /// Get subscriptions for authenticated organization
        /// </summary>
        [HttpGet("my-organization")]
        public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetMyOrganizationSubscriptions()
        {
            if (!_organizationContext.OrganizationId.HasValue)
            {
                return BadRequest("Organization context not found");
            }

            try
            {
                var subscriptions = await _subscriptionService.GetByOrganizationIdAsync(_organizationContext.OrganizationId.Value);
                return Ok(subscriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subscriptions for organization: {OrganizationId}", _organizationContext.OrganizationId);
                return BadRequest("Error retrieving subscriptions");
            }
        }

        /// <summary>
        /// Get subscriptions by status
        /// </summary>
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetSubscriptionsByStatus(SubscriptionStatusDto status)
        {
            try
            {
                var subscriptions = await _subscriptionService.GetByStatusAsync(status);
                return Ok(subscriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving subscriptions by status: {Status}", status);
                return BadRequest("Error retrieving subscriptions");
            }
        }

        /// <summary>
        /// Get active subscriptions
        /// </summary>
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetActiveSubscriptions()
        {
            try
            {
                var subscriptions = await _subscriptionService.GetActiveSubscriptionsAsync();
                return Ok(subscriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active subscriptions");
                return BadRequest("Error retrieving active subscriptions");
            }
        }

        /// <summary>
        /// Get expiring subscriptions
        /// </summary>
        [HttpGet("expiring")]
        public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetExpiringSubscriptions([FromQuery] DateTime? expiryDate = null)
        {
            var targetDate = expiryDate ?? DateTime.UtcNow.AddDays(30); // Default to 30 days

            try
            {
                var subscriptions = await _subscriptionService.GetExpiringSubscriptionsAsync(targetDate);
                return Ok(subscriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving expiring subscriptions");
                return BadRequest("Error retrieving expiring subscriptions");
            }
        }

        /// <summary>
        /// Get expired subscriptions
        /// </summary>
        [HttpGet("expired")]
        public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetExpiredSubscriptions()
        {
            try
            {
                var subscriptions = await _subscriptionService.GetExpiredSubscriptionsAsync();
                return Ok(subscriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving expired subscriptions");
                return BadRequest("Error retrieving expired subscriptions");
            }
        }

        /// <summary>
        /// Extend subscription
        /// </summary>
        [HttpPost("{id}/extend")]
        public async Task<IActionResult> ExtendSubscription(int id, [FromBody] DateTime newEndDate)
        {
            try
            {
                var result = await _subscriptionService.ExtendSubscriptionAsync(id, newEndDate);
                if (!result)
                {
                    return NotFound();
                }

                _logger.LogInformation("Subscription {SubscriptionId} extended to {EndDate}", id, newEndDate);
                
                return Ok(new { message = "Subscription extended successfully", newEndDate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending subscription: {SubscriptionId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Cancel subscription
        /// </summary>
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelSubscription(int id)
        {
            try
            {
                var result = await _subscriptionService.CancelSubscriptionAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                _logger.LogInformation("Subscription {SubscriptionId} cancelled", id);
                
                return Ok(new { message = "Subscription cancelled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling subscription: {SubscriptionId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Renew subscription
        /// </summary>
        [HttpPost("{id}/renew")]
        public async Task<IActionResult> RenewSubscription(int id)
        {
            try
            {
                var result = await _subscriptionService.RenewSubscriptionAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                _logger.LogInformation("Subscription {SubscriptionId} renewed", id);
                
                return Ok(new { message = "Subscription renewed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error renewing subscription: {SubscriptionId}", id);
                return BadRequest(ex.Message);
            }
        }
    }
}