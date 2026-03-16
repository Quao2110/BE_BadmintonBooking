using Application.DTOs.ResponseDTOs.User;

namespace Application.DTOs.ResponseDTOs.Order;

public class OrderResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public double? DeliveryLatitude { get; set; }
    public double? DeliveryLongitude { get; set; }
    public decimal? TotalAmount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty; // Pending, Paid, Failed
    public string OrderStatus { get; set; } = string.Empty; // Pending, Confirmed, Shipping, Delivered, Cancelled
    public DateTime? OrderDate { get; set; }
    public List<OrderDetailResponse> OrderDetails { get; set; } = new();
    public UserResponse? User { get; set; }
}

