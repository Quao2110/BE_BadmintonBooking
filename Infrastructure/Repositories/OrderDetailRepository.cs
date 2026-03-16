using Application.Interfaces.IRepositories;
using Domain.Entities;
using Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class OrderDetailRepository : GenericRepository<OrderDetail>, IOrderDetailRepository
{
    private readonly BadmintonBooking_PRM393Context _context;

    public OrderDetailRepository(BadmintonBooking_PRM393Context context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrderDetail>> GetByOrderIdAsync(Guid orderId)
    {
        return await _context.OrderDetails
            .Include(od => od.Product)
            .Where(od => od.OrderId == orderId)
            .ToListAsync();
    }
}

