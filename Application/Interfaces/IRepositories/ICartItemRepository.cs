using Domain.Entities;

namespace Application.Interfaces.IRepositories;

public interface ICartItemRepository : IGenericRepository<CartItem>
{
    Task<CartItem?> GetByCartIdAndProductIdAsync(Guid cartId, Guid productId);
    Task<IEnumerable<CartItem>> GetByCartIdAsync(Guid cartId);
}

