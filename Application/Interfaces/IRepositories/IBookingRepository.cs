using Domain.Entities;

namespace Application.Interfaces.IRepositories;

public interface IBookingRepository : IGenericRepository<Booking>
{
    Task<bool> ExistsOverlappingAsync(Guid courtId, DateTime startTime, DateTime endTime, IEnumerable<string> statuses, Guid? excludeBookingId = null);
    Task<List<Booking>> GetCourtBookingsInRangeAsync(Guid courtId, DateTime startTime, DateTime endTime, IEnumerable<string> statuses);
    Task<Booking?> GetByIdWithDetailsAsync(Guid bookingId);
    Task<(IEnumerable<Booking> Items, int TotalItems)> GetUserHistoryAsync(Guid userId, string? status, DateTime? fromDate, DateTime? toDate, int page, int pageSize);
}
