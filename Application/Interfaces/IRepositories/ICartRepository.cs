using Domain.Entities;

namespace Application.Interfaces.IRepositories;

public interface ICartRepository : IGenericRepository<Cart>
{
    Task<Cart?> GetByUserIdWithIncludesAsync(Guid userId);
    Task<Cart?> GetCartByIdWithIncludesAsync(Guid cartId);
}

