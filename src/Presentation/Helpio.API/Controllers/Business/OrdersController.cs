using Microsoft.AspNetCore.Mvc;
using Helpio.Ir.Application.Services.Business;
using Helpio.Ir.Application.DTOs.Business;
using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.API.Services;
using FluentValidation;

namespace Helpio.Ir.API.Controllers.Business
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IOrganizationContext _organizationContext;
        private readonly IValidator<CreateOrderDto> _createValidator;
        private readonly IValidator<UpdateOrderDto> _updateValidator;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            IOrderService orderService,
            IOrganizationContext organizationContext,
            IValidator<CreateOrderDto> createValidator,
            IValidator<UpdateOrderDto> updateValidator,
            ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _organizationContext = organizationContext;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _logger = logger;
        }

        /// <summary>
        /// Get all orders with pagination
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<OrderDto>>> GetOrders([FromQuery] PaginationRequest request)
        {
            try
            {
                var result = await _orderService.GetOrdersAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders");
                return BadRequest("Error retrieving orders");
            }
        }

        /// <summary>
        /// Get order by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        /// <summary>
        /// Get order by order number
        /// </summary>
        [HttpGet("by-number/{orderNumber}")]
        public async Task<ActionResult<OrderDto>> GetOrderByNumber(string orderNumber)
        {
            var order = await _orderService.GetByOrderNumberAsync(orderNumber);
            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        /// <summary>
        /// Create a new order
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderDto createDto)
        {
            // Validate input
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _orderService.CreateAsync(createDto);
                
                _logger.LogInformation("Order created: {OrderNumber} for Customer: {CustomerId}", 
                    result.OrderNumber, createDto.CustomerId);

                return CreatedAtAction(nameof(GetOrder), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for Customer: {CustomerId}", createDto.CustomerId);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Update order
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<OrderDto>> UpdateOrder(int id, UpdateOrderDto updateDto)
        {
            // Validate input
            var validationResult = await _updateValidator.ValidateAsync(updateDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            try
            {
                var result = await _orderService.UpdateAsync(id, updateDto);
                
                _logger.LogInformation("Order updated: {OrderId}", id);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order: {OrderId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete order
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                var result = await _orderService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound();
                }

                _logger.LogInformation("Order deleted: {OrderId}", id);
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order: {OrderId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get orders by customer ID
        /// </summary>
        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByCustomer(int customerId)
        {
            try
            {
                var orders = await _orderService.GetByCustomerIdAsync(customerId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for customer: {CustomerId}", customerId);
                return BadRequest("Error retrieving orders");
            }
        }

        /// <summary>
        /// Get orders by subscription ID
        /// </summary>
        [HttpGet("subscription/{subscriptionId}")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersBySubscription(int subscriptionId)
        {
            try
            {
                var orders = await _orderService.GetBySubscriptionIdAsync(subscriptionId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for subscription: {SubscriptionId}", subscriptionId);
                return BadRequest("Error retrieving orders");
            }
        }

        /// <summary>
        /// Get orders by status
        /// </summary>
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByStatus(OrderStatusDto status)
        {
            try
            {
                var orders = await _orderService.GetByStatusAsync(status);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders by status: {Status}", status);
                return BadRequest("Error retrieving orders");
            }
        }

        /// <summary>
        /// Get orders within a date range
        /// </summary>
        [HttpGet("date-range")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByDateRange(
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            try
            {
                var orders = await _orderService.GetByDateRangeAsync(startDate, endDate);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders by date range: {StartDate} to {EndDate}", startDate, endDate);
                return BadRequest("Error retrieving orders");
            }
        }

        /// <summary>
        /// Get pending orders
        /// </summary>
        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetPendingOrders()
        {
            try
            {
                var orders = await _orderService.GetPendingOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pending orders");
                return BadRequest("Error retrieving pending orders");
            }
        }

        /// <summary>
        /// Update order status
        /// </summary>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] OrderStatusDto status)
        {
            try
            {
                var result = await _orderService.UpdateStatusAsync(id, status);
                if (!result)
                {
                    return NotFound();
                }

                _logger.LogInformation("Order {OrderId} status updated to {Status}", id, status);
                
                return Ok(new { message = "Order status updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status: {OrderId}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get total revenue
        /// </summary>
        [HttpGet("revenue/total")]
        public async Task<ActionResult<decimal>> GetTotalRevenue()
        {
            try
            {
                var revenue = await _orderService.GetTotalRevenueAsync();
                return Ok(new { totalRevenue = revenue });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving total revenue");
                return BadRequest("Error retrieving revenue data");
            }
        }

        /// <summary>
        /// Get revenue statistics
        /// </summary>
        [HttpGet("revenue/statistics")]
        public async Task<ActionResult<Dictionary<string, decimal>>> GetRevenueStatistics()
        {
            try
            {
                var statistics = await _orderService.GetRevenueStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving revenue statistics");
                return BadRequest("Error retrieving revenue statistics");
            }
        }
    }
}