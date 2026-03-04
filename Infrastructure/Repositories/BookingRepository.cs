using Application.Interfaces.IRepositories;
using Domain.Entities;
using Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class BookingRepository : GenericRepository<Booking>, IBookingRepository
{
    private readonly BadmintonBooking_PRM393Context _context;

    public BookingRepository(BadmintonBooking_PRM393Context context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> ExistsOverlappingAsync(Guid courtId, DateTime startTime, DateTime endTime, IEnumerable<string> statuses, Guid? excludeBookingId = null)
    {
        var statusSet = statuses.Select(s => s.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var query = _context.Bookings
            .Where(b => b.CourtId == courtId)
            .Where(b => statusSet.Contains(b.Status))
            .Where(b => b.StartTime < endTime && b.EndTime > startTime);

        if (excludeBookingId.HasValue)
        {
            query = query.Where(b => b.Id != excludeBookingId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<List<Booking>> GetCourtBookingsInRangeAsync(Guid courtId, DateTime startTime, DateTime endTime, IEnumerable<string> statuses)
    {
        var statusSet = statuses.Select(s => s.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);

        return await _context.Bookings
            .Where(b => b.CourtId == courtId)
            .Where(b => statusSet.Contains(b.Status))
            .Where(b => b.StartTime < endTime && b.EndTime > startTime)
            .ToListAsync();
    }

    public async Task<Booking?> GetByIdWithDetailsAsync(Guid bookingId)
    {
        return await _context.Bookings
            .Include(b => b.Court)
            .Include(b => b.BookingServices)
                .ThenInclude(bs => bs.Service)
            .FirstOrDefaultAsync(b => b.Id == bookingId);
    }

    public async Task<(IEnumerable<Booking> Items, int TotalItems)> GetUserHistoryAsync(Guid userId, string? status, DateTime? fromDate, DateTime? toDate, int page, int pageSize)
    {
        var query = _context.Bookings
            .Include(b => b.Court)
            .Where(b => b.UserId == userId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalized = status.Trim();
            query = query.Where(b => b.Status == normalized);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(b => b.StartTime >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(b => b.EndTime <= toDate.Value);
        }

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalItems);
    }
}
