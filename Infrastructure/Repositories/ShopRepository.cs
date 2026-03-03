using Application.Interfaces.IRepositories;
using Domain.Entities;
using Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ShopRepository : GenericRepository<Shop>, IShopRepository
{
    private readonly BadmintonBooking_PRM393Context _context;

    public ShopRepository(BadmintonBooking_PRM393Context context) : base(context)
    {
        _context = context;
    }

    public async Task<Shop?> GetFirstAsync()
    {
        return await _context.Shops
            .Include(s => s.ShopImages)
            .FirstOrDefaultAsync();
    }
}
