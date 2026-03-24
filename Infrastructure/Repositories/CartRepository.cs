using Application.Interfaces.IRepositories;
using Domain.Entities;
using Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CartRepository : GenericRepository<Cart>, ICartRepository
{
    private readonly BadmintonBooking_PRM393Context _context;

    public CartRepository(BadmintonBooking_PRM393Context context) : base(context)
    {
        _context = context;
    }

    public async Task<Cart?> GetByUserIdWithIncludesAsync(Guid userId)
    {
        return await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p.Category)
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p.ProductImages)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<Cart?> GetCartByIdWithIncludesAsync(Guid cartId)
    {
        return await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p.Category)
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p.ProductImages)
            .FirstOrDefaultAsync(c => c.Id == cartId);
    }
}

