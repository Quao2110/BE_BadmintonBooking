using Application.Interfaces.IRepositories;
using Domain.Entities;
using Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CartItemRepository : GenericRepository<CartItem>, ICartItemRepository
{
    private readonly BadmintonBooking_PRM393Context _context;

    public CartItemRepository(BadmintonBooking_PRM393Context context) : base(context)
    {
        _context = context;
    }

    public async Task<CartItem?> GetByCartIdAndProductIdAsync(Guid cartId, Guid productId)
    {
        return await _context.CartItems
            .Include(ci => ci.Product)
            .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductId == productId);
    }

    public async Task<IEnumerable<CartItem>> GetByCartIdAsync(Guid cartId)
    {
        return await _context.CartItems
            .Include(ci => ci.Product)
            .Where(ci => ci.CartId == cartId)
            .ToListAsync();
    }
}

