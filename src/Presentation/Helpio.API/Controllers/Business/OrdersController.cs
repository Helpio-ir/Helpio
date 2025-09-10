using Microsoft.AspNetCore.Mvc;
using Helpio.Ir.Application.Services.Business;
using Helpio.Ir.Application.Services.Core;
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
        private readonly ICustomerService _customerService;
        private readonly IOrganizationContext _organizationContext;
        private readonly IValidator<CreateOrderDto> _createValidator;
        private readonly IValidator<UpdateOrderDto> _updateValidator;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            IOrderService orderService,
            ICustomerService customerService,
            IOrganizationContext organizationContext,
            IValidator<CreateOrderDto> createValidator,
            IValidator<UpdateOrderDto> updateValidator,
            ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _customerService = customerService;
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
                var result = await _orderService.GetOrdersAsync(request);
                
                // ????? ??????? ?? ???? ??????? ?????? ????
                // ????: Orders ???? ?? ???? Customer ?? Organization ????? ????
                var filteredOrders = result.Items.Where(o => HasOrderAccess(o));
                result.Items = filteredOrders;
                result.TotalItems = filteredOrders.Count();

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
            // ????? ????? ????
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            if (!_organizationContext.OrganizationId.HasValue)
            {
                return BadRequest("Organization context not found");
            }

            var order = await _orderService.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // ????? ?????? ???????
            if (!HasOrderAccess(order))
            {
                return Forbid("Access denied to this order");
            }

            return Ok(order);
        }

        /// <summary>
        /// Get order by order number
        /// </summary>
        [HttpGet("by-number/{orderNumber}")]
        public async Task<ActionResult<OrderDto>> GetOrderByNumber(string orderNumber)
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

            var order = await _orderService.GetByOrderNumberAsync(orderNumber);
            if (order == null)
            {
                return NotFound();
            }

            // ????? ?????? ???????
            if (!HasOrderAccess(order))
            {
                return Forbid("Access denied to this order");
            }

            return Ok(order);
        }

        /// <summary>
        /// Create a new order
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderDto createDto)
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

            // Validate input
            var validationResult = await _createValidator.ValidateAsync(createDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            // TODO: ????? ????? Customer ? Subscription ????? ?? ???? ?????? ?????
            if (!await ValidateOrderEntitiesAccess(createDto.CustomerId, createDto.SubscriptionId))
            {
                return Forbid("Access denied to specified customer or subscription");
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
            // ????? ????? ????
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            if (!_organizationContext.OrganizationId.HasValue)
            {
                return BadRequest("Organization context not found");
            }

            // ????? ???? ????? ? ?????? ???????
            var existingOrder = await _orderService.GetByIdAsync(id);
            if (existingOrder == null)
            {
                return NotFound();
            }

            if (!HasOrderAccess(existingOrder))
            {
                return Forbid("Access denied to this order");
            }

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
        /// Delete order (????? ???)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
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

            // ????? ???? ????? ? ?????? ???????
            var existingOrder = await _orderService.GetByIdAsync(id);
            if (existingOrder == null)
            {
                return NotFound();
            }

            if (!HasOrderAccess(existingOrder))
            {
                return Forbid("Access denied to this order");
            }

            // ???????: ??? ??????? Pending ???? ??? ?????
            if (existingOrder.Status != OrderStatusDto.Pending)
            {
                return BadRequest("Only pending orders can be deleted");
            }

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
                var orders = await _orderService.GetByCustomerIdAsync(customerId);
                
                // ????? ?? ???? ?????? ???????
                var filteredOrders = orders.Where(o => HasOrderAccess(o));

                return Ok(filteredOrders);
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
                var orders = await _orderService.GetBySubscriptionIdAsync(subscriptionId);
                
                // ????? ?? ???? ?????? ???????
                var filteredOrders = orders.Where(o => HasOrderAccess(o));

                return Ok(filteredOrders);
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
                var orders = await _orderService.GetByStatusAsync(status);
                
                // ????? ?? ???? ?????? ???????
                var filteredOrders = orders.Where(o => HasOrderAccess(o));

                return Ok(filteredOrders);
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
                var orders = await _orderService.GetByDateRangeAsync(startDate, endDate);
                
                // ????? ?? ???? ?????? ???????
                var filteredOrders = orders.Where(o => HasOrderAccess(o));

                return Ok(filteredOrders);
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
                var orders = await _orderService.GetPendingOrdersAsync();
                
                // ????? ?? ???? ?????? ???????
                var filteredOrders = orders.Where(o => HasOrderAccess(o));

                return Ok(filteredOrders);
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
            // ????? ????? ????
            if (!_organizationContext.IsAuthenticated)
            {
                return Unauthorized("User must be authenticated");
            }

            if (!_organizationContext.OrganizationId.HasValue)
            {
                return BadRequest("Organization context not found");
            }

            // ????? ???? ????? ? ?????? ???????
            var existingOrder = await _orderService.GetByIdAsync(id);
            if (existingOrder == null)
            {
                return NotFound();
            }

            if (!HasOrderAccess(existingOrder))
            {
                return Forbid("Access denied to this order");
            }

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
        /// Get organization revenue (????? ???)
        /// </summary>
        [HttpGet("revenue/organization")]
        public async Task<ActionResult<decimal>> GetOrganizationRevenue()
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
                // ??? ????? ?????? ????
                var allOrders = await _orderService.GetOrdersAsync(new PaginationRequest { PageNumber = 1, PageSize = int.MaxValue });
                var organizationOrders = allOrders.Items.Where(o => HasOrderAccess(o));
                var revenue = organizationOrders.Where(o => o.IsCompleted).Sum(o => o.NetAmount);

                return Ok(new { organizationRevenue = revenue });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving organization revenue");
                return BadRequest("Error retrieving revenue data");
            }
        }

        /// <summary>
        /// Get organization revenue statistics (????? ???)
        /// </summary>
        [HttpGet("revenue/organization-statistics")]
        public async Task<ActionResult> GetOrganizationRevenueStatistics()
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
                // ??? ???? ?????? ????
                var allOrders = await _orderService.GetOrdersAsync(new PaginationRequest { PageNumber = 1, PageSize = int.MaxValue });
                var organizationOrders = allOrders.Items.Where(o => HasOrderAccess(o)).ToList();

                var statistics = new Dictionary<string, decimal>
                {
                    ["TotalRevenue"] = organizationOrders.Where(o => o.IsCompleted).Sum(o => o.NetAmount),
                    ["PendingRevenue"] = organizationOrders.Where(o => o.Status == OrderStatusDto.Pending).Sum(o => o.NetAmount),
                    ["MonthlyRevenue"] = organizationOrders.Where(o => o.OrderDate >= DateTime.UtcNow.AddDays(-30) && o.IsCompleted).Sum(o => o.NetAmount),
                    ["AverageOrderValue"] = organizationOrders.Any() ? organizationOrders.Average(o => o.NetAmount) : 0
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving organization revenue statistics");
                return BadRequest("Error retrieving revenue statistics");
            }
        }

        /// <summary>
        /// ????? ?????? ????? ?? ????? ?? ???? ??????
        /// </summary>
        private bool HasOrderAccess(OrderDto order)
        {
            if (!_organizationContext.OrganizationId.HasValue)
                return false;

            // ????? ?????? ?? ???? Customer.OrganizationId
            if (order.Customer?.OrganizationId == _organizationContext.OrganizationId.Value)
                return true;

            // ????? ?????? ?? ???? Subscription.OrganizationId
            if (order.Subscription?.OrganizationId == _organizationContext.OrganizationId.Value)
                return true;

            return false;
        }

        /// <summary>
        /// ????? ?????? ?? Customer ? Subscription ????? ????? ?????
        /// </summary>
        private async Task<bool> ValidateOrderEntitiesAccess(int customerId, int subscriptionId)
        {
            if (!_organizationContext.OrganizationId.HasValue)
                return false;

            try
            {
                // ????? Customer
                var customer = await _customerService.GetByIdAsync(customerId);
                if (customer?.OrganizationId != _organizationContext.OrganizationId.Value)
                    return false;

                // ????? Subscription (??? ???? ????)
                if (subscriptionId > 0)
                {
                    // TODO: ????? ???? service ???? Subscription
                    // var subscription = await _subscriptionService.GetByIdAsync(subscriptionId);
                    // if (subscription?.OrganizationId != _organizationContext.OrganizationId.Value)
                    //     return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}