using Infrastructure.DbContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebAPI.Controllers;

[Route("api/dashboard")]
[ApiController]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase
{
    private readonly BadmintonBooking_PRM393Context _db;

    public DashboardController(BadmintonBooking_PRM393Context db)
    {
        _db = db;
    }

    [HttpGet("bookings/revenue")]
    public async Task<IActionResult> BookingRevenue([FromQuery] string period = "day")
    {
        // period: day, month, year
        var bookings = _db.Bookings.AsQueryable();

        if (period == "day")
        {
            var data = await bookings
                .GroupBy(b => b.CreatedAt.HasValue ? b.CreatedAt.Value.Date : DateTime.MinValue)
                .Select(g => new { date = g.Key, revenue = g.Sum(x => x.TotalPrice ?? 0), count = g.Count() })
                .OrderBy(d => d.date)
                .ToListAsync();
            return Ok(data);
        }
        else if (period == "month")
        {
            var data = await bookings
                .GroupBy(b => new { year = b.CreatedAt.Value.Year, month = b.CreatedAt.Value.Month })
                .Select(g => new { year = g.Key.year, month = g.Key.month, revenue = g.Sum(x => x.TotalPrice ?? 0), count = g.Count() })
                .OrderBy(d => d.year).ThenBy(d => d.month)
                .ToListAsync();
            return Ok(data);
        }
        else // year
        {
            var data = await bookings
                .GroupBy(b => b.CreatedAt.Value.Year)
                .Select(g => new { year = g.Key, revenue = g.Sum(x => x.TotalPrice ?? 0), count = g.Count() })
                .OrderBy(d => d.year)
                .ToListAsync();
            return Ok(data);
        }
    }

    [HttpGet("orders/revenue")]
    public async Task<IActionResult> OrdersRevenue()
    {
        var orders = _db.Orders.AsQueryable();

        var totalRevenue = await orders.SumAsync(o => o.TotalAmount ?? 0);
        var totalOrders = await orders.CountAsync();

        var top5 = await _db.OrderDetails
            .Include(od => od.Product)
            .GroupBy(od => new { od.ProductId, od.Product.ProductName })
            .Select(g => new { productId = g.Key.ProductId, productName = g.Key.ProductName, quantity = g.Sum(x => x.Quantity), revenue = g.Sum(x => (x.UnitPrice) * x.Quantity) })
            .OrderByDescending(x => x.quantity)
            .Take(5)
            .ToListAsync();

        return Ok(new { totalRevenue, totalOrders, top5 });
    }
}
