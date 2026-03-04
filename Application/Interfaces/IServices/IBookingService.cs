using Application.DTOs.RequestDTOs.Booking;
using Application.DTOs.ResponseDTOs.Booking;
using Application.DTOs.ResponseDTOs.Common;

namespace Application.Interfaces.IServices;

public interface IBookingService
{
    Task<BookingAvailabilityResponse> GetAvailabilityAsync(Guid courtId, DateTime date);
    Task<BookingResponse> CreateAsync(Guid userId, BookingCreateRequest request);
    Task<PagedResult<BookingHistoryItemResponse>> GetMyHistoryAsync(Guid userId, BookingHistoryQuery query);
    Task<BookingResponse> UpdateStatusAsync(Guid bookingId, string status);
    Task<BookingResponse> CancelMyBookingAsync(Guid userId, Guid bookingId);
}
