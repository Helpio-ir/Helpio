using AutoMapper;
using Microsoft.Extensions.Logging;
using Helpio.Ir.Application.DTOs.Business;
using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.Application.Services.Business;
using Helpio.Ir.Application.Common.Exceptions;
using Helpio.Ir.Domain.Interfaces;
using Helpio.Ir.Domain.Entities.Business;

namespace Helpio.Ir.Application.Services.Business
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<OrderService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<OrderDto?> GetByIdAsync(int id)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            return order != null ? _mapper.Map<OrderDto>(order) : null;
        }

        public async Task<OrderDto?> GetByOrderNumberAsync(string orderNumber)
        {
            var order = await _unitOfWork.Orders.GetByOrderNumberAsync(orderNumber);
            return order != null ? _mapper.Map<OrderDto>(order) : null;
        }

        public async Task<PaginatedResult<OrderDto>> GetOrdersAsync(PaginationRequest request)
        {
            var orders = await _unitOfWork.Orders.GetAllAsync();

            // Apply search filter
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                orders = orders.Where(o =>
                    o.OrderNumber.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    (o.Notes != null && o.Notes.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase)));
            }

            // Apply sorting
            orders = request.SortBy?.ToLower() switch
            {
                "ordernumber" => request.SortDescending ? orders.OrderByDescending(o => o.OrderNumber) : orders.OrderBy(o => o.OrderNumber),
                "totalamount" => request.SortDescending ? orders.OrderByDescending(o => o.TotalAmount) : orders.OrderBy(o => o.TotalAmount),
                "orderdate" => request.SortDescending ? orders.OrderByDescending(o => o.OrderDate) : orders.OrderBy(o => o.OrderDate),
                "status" => request.SortDescending ? orders.OrderByDescending(o => o.Status) : orders.OrderBy(o => o.Status),
                _ => orders.OrderByDescending(o => o.OrderDate)
            };

            var totalItems = orders.Count();
            var items = orders
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var orderDtos = _mapper.Map<List<OrderDto>>(items);

            return new PaginatedResult<OrderDto>
            {
                Items = orderDtos,
                TotalItems = totalItems,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<OrderDto> CreateAsync(CreateOrderDto createDto)
        {
            // Validate customer exists
            var customer = await _unitOfWork.Customers.GetByIdAsync(createDto.CustomerId);
            if (customer == null)
            {
                throw new NotFoundException("Customer", createDto.CustomerId);
            }

            // Validate subscription exists
            var subscription = await _unitOfWork.Subscriptions.GetByIdAsync(createDto.SubscriptionId);
            if (subscription == null)
            {
                throw new NotFoundException("Subscription", createDto.SubscriptionId);
            }

            var order = _mapper.Map<Order>(createDto);
            order.OrderNumber = GenerateOrderNumber();

            var createdOrder = await _unitOfWork.Orders.AddAsync(order);

            _logger.LogInformation("Order created with ID: {OrderId}, Number: {OrderNumber}", 
                createdOrder.Id, createdOrder.OrderNumber);

            return _mapper.Map<OrderDto>(createdOrder);
        }

        public async Task<OrderDto> UpdateAsync(int id, UpdateOrderDto updateDto)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
            {
                throw new NotFoundException("Order", id);
            }

            _mapper.Map(updateDto, order);
            await _unitOfWork.Orders.UpdateAsync(order);

            _logger.LogInformation("Order updated with ID: {OrderId}", id);

            return _mapper.Map<OrderDto>(order);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
            {
                return false;
            }

            await _unitOfWork.Orders.DeleteAsync(order);

            _logger.LogInformation("Order deleted with ID: {OrderId}", id);

            return true;
        }

        public async Task<IEnumerable<OrderDto>> GetByCustomerIdAsync(int customerId)
        {
            var orders = await _unitOfWork.Orders.GetByCustomerIdAsync(customerId);
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<IEnumerable<OrderDto>> GetBySubscriptionIdAsync(int subscriptionId)
        {
            var orders = await _unitOfWork.Orders.GetBySubscriptionIdAsync(subscriptionId);
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<IEnumerable<OrderDto>> GetByStatusAsync(OrderStatusDto status)
        {
            var domainStatus = _mapper.Map<OrderStatus>(status);
            var orders = await _unitOfWork.Orders.GetByStatusAsync(domainStatus);
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<IEnumerable<OrderDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var orders = await _unitOfWork.Orders.GetByDateRangeAsync(startDate, endDate);
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _unitOfWork.Orders.GetTotalRevenueAsync();
        }

        public async Task<IEnumerable<OrderDto>> GetPendingOrdersAsync()
        {
            var orders = await _unitOfWork.Orders.GetPendingOrdersAsync();
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<bool> UpdateStatusAsync(int orderId, OrderStatusDto status)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
            {
                throw new NotFoundException("Order", orderId);
            }

            order.Status = _mapper.Map<OrderStatus>(status);
            
            // Set dates based on status
            switch (status)
            {
                case OrderStatusDto.Shipped:
                    order.ShippedDate = DateTime.UtcNow;
                    break;
                case OrderStatusDto.Delivered:
                    order.DeliveredDate = DateTime.UtcNow;
                    break;
            }

            await _unitOfWork.Orders.UpdateAsync(order);

            _logger.LogInformation("Order {OrderId} status updated to {Status}", orderId, status);

            return true;
        }

        public async Task<Dictionary<string, decimal>> GetRevenueStatisticsAsync()
        {
            var allOrders = await _unitOfWork.Orders.GetAllAsync();
            var completedOrders = allOrders.Where(o => o.Status == OrderStatus.Delivered);

            return new Dictionary<string, decimal>
            {
                ["TotalRevenue"] = completedOrders.Sum(o => o.TotalAmount),
                ["MonthlyRevenue"] = completedOrders.Where(o => o.OrderDate >= DateTime.UtcNow.AddMonths(-1)).Sum(o => o.TotalAmount),
                ["YearlyRevenue"] = completedOrders.Where(o => o.OrderDate >= DateTime.UtcNow.AddYears(-1)).Sum(o => o.TotalAmount),
                ["AverageOrderValue"] = completedOrders.Any() ? completedOrders.Average(o => o.TotalAmount) : 0
            };
        }

        private static string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        }
    }
}