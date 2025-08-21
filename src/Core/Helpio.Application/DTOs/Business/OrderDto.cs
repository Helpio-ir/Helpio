using Helpio.Ir.Application.DTOs.Common;
using Helpio.Ir.Application.DTOs.Core;

namespace Helpio.Ir.Application.DTOs.Business
{
    public enum OrderStatusDto
    {
        Pending = 1,
        Confirmed = 2,
        Processing = 3,
        Shipped = 4,
        Delivered = 5,
        Cancelled = 6,
        Refunded = 7
    }

    public class OrderDto : BaseDto
    {
        public int SubscriptionId { get; set; }
        public int CustomerId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public OrderStatusDto Status { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public string? Notes { get; set; }
        
        // Navigation DTOs
        public SubscriptionDto? Subscription { get; set; }
        public CustomerDto? Customer { get; set; }
        
        // Computed Properties
        public decimal NetAmount => TotalAmount - TaxAmount - DiscountAmount;
        public bool IsCompleted => Status == OrderStatusDto.Delivered;
        public TimeSpan? ProcessingTime => DeliveredDate.HasValue ? DeliveredDate - OrderDate : null;
    }

    public class CreateOrderDto
    {
        public int SubscriptionId { get; set; }
        public int CustomerId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateOrderDto
    {
        public OrderStatusDto Status { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public string? Notes { get; set; }
    }
}