using Application.DTOs.RequestDTOs.Order;
using Application.DTOs.ResponseDTOs.Common;
using Application.DTOs.ResponseDTOs.Order;

namespace Application.Interfaces.IServices;

public interface IOrderService
{
    Task<OrderResponse> CheckoutAsync(Guid userId, CheckoutRequest request);
    Task<OrderResponse> GetOrderByIdAsync(Guid orderId);
    Task<IEnumerable<OrderResponse>> GetOrdersByUserAsync(Guid userId);
    Task<PagedResult<OrderResponse>> GetOrdersPagedAsync(
        Guid? userId,
        string? orderStatus,
        string? paymentStatus,
        int page = 1,
        int pageSize = 10);
    Task<OrderResponse> UpdateOrderStatusAsync(Guid orderId, string newStatus);
    Task<OrderResponse> CancelOrderAsync(Guid orderId);
}

