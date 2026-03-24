using Domain.Entities;

namespace Application.Interfaces.IRepositories;

public interface IOrderDetailRepository : IGenericRepository<OrderDetail>
{
    Task<IEnumerable<OrderDetail>> GetByOrderIdAsync(Guid orderId);
}

