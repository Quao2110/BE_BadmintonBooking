using Domain.Entities;

namespace Application.Interfaces.IRepositories;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId);
    Task<Order?> GetByIdWithIncludesAsync(Guid orderId);
    Task<(IEnumerable<Order> Items, int TotalItems)> GetPagedAsync(
        Guid? userId,
        string? orderStatus,
        string? paymentStatus,
        int page,
        int pageSize);
}

