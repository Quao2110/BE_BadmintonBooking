using Application.Constants;
using Application.DTOs.RequestDTOs.Booking;
using Application.DTOs.ResponseDTOs.Booking;
using Application.DTOs.ResponseDTOs.Common;
using Application.Interfaces.IServices;
using Application.Interfaces.IUnitOfWork;
using Application.Options;
using Domain.Entities;
using Microsoft.Extensions.Options;
using BookingServiceEntity = Domain.Entities.BookingService;

namespace Application.Services;

public class BookingService : IBookingService
{
    private const int MaxPageSize = 100;
    private readonly IUnitOfWork _unitOfWork;
    private readonly BookingOptions _options;

    public BookingService(IUnitOfWork unitOfWork, IOptions<BookingOptions> options)
    {
        _unitOfWork = unitOfWork;
        _options = options.Value;
    }

    public async Task<BookingAvailabilityResponse> GetAvailabilityAsync(Guid courtId, DateTime date)
    {
        if (date == default)
        {
            throw new Exception("Date is required.");
        }

        var court = await _unitOfWork.CourtRepository.GetByIdAsync(courtId)
            ?? throw new Exception("Court not found.");

        if (!string.Equals(court.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("Court is not active.");
        }

        var (openTime, closeTime, slotMinutes) = ParseAndValidateBookingOptions();

        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);

        var blockingBookings = await _unitOfWork.BookingRepository.GetCourtBookingsInRangeAsync(
            courtId,
            dayStart,
            dayEnd,
            BookingStatus.BlockingStatuses);

        var slots = new List<BookingAvailabilitySlotResponse>();
        for (var slotStart = dayStart.Add(openTime); slotStart < dayStart.Add(closeTime); slotStart = slotStart.AddMinutes(slotMinutes))
        {
            var slotEnd = slotStart.AddMinutes(slotMinutes);
            var isBusy = blockingBookings.Any(b => b.StartTime < slotEnd && b.EndTime > slotStart);

            slots.Add(new BookingAvailabilitySlotResponse
            {
                StartTime = slotStart,
                EndTime = slotEnd,
                IsAvailable = !isBusy
            });
        }

        return new BookingAvailabilityResponse
        {
            CourtId = courtId,
            Date = dayStart,
            OpenTime = _options.OpenTime,
            CloseTime = _options.CloseTime,
            SlotMinutes = slotMinutes,
            Slots = slots
        };
    }

    public async Task<BookingResponse> CreateAsync(Guid userId, BookingCreateRequest request)
    {
        ValidateCreateRequest(request);

        var court = await _unitOfWork.CourtRepository.GetByIdAsync(request.CourtId)
            ?? throw new Exception("Court not found.");

        if (!string.Equals(court.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("Court is not active.");
        }

        var (_, _, slotMinutes) = ParseAndValidateBookingOptions();
        ValidateBookingTimeWindow(request.StartTime, request.EndTime, slotMinutes);

        var hasOverlap = await _unitOfWork.BookingRepository.ExistsOverlappingAsync(
            request.CourtId,
            request.StartTime,
            request.EndTime,
            BookingStatus.BlockingStatuses);

        if (hasOverlap)
        {
            throw new Exception("Booking time overlaps with an existing booking.");
        }

        var serviceItems = request.ServiceItems?.Where(x => x.Quantity > 0).ToList() ?? [];
        var serviceMap = new Dictionary<Guid, Service>();

        foreach (var item in serviceItems)
        {
            var service = await _unitOfWork.ServiceRepository.GetByIdAsync(item.ServiceId)
                ?? throw new Exception($"Service with id '{item.ServiceId}' not found.");

            if (service.IsActive == false)
            {
                throw new Exception($"Service '{service.ServiceName}' is not active.");
            }

            serviceMap[item.ServiceId] = service;
        }

        var totalHours = (decimal)(request.EndTime - request.StartTime).TotalHours;
        var courtPrice = totalHours * _options.CourtHourlyRate;

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CourtId = request.CourtId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Status = BookingStatus.Pending,
            IsPaid = false,
            CreatedAt = DateTime.UtcNow
        };

        decimal servicesPrice = 0;
        foreach (var item in serviceItems)
        {
            var service = serviceMap[item.ServiceId];
            var linePrice = service.Price * item.Quantity;
            servicesPrice += linePrice;

            booking.BookingServices.Add(new BookingServiceEntity
            {
                Id = Guid.NewGuid(),
                ServiceId = item.ServiceId,
                Quantity = item.Quantity,
                PriceAtBooking = service.Price
            });
        }

        booking.TotalPrice = courtPrice + servicesPrice;

        await _unitOfWork.BookingRepository.CreateAsync(booking);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.BookingRepository.GetByIdWithDetailsAsync(booking.Id)
            ?? throw new Exception("Cannot load created booking.");

        return ToBookingResponse(created);
    }

    public async Task<PagedResult<BookingHistoryItemResponse>> GetMyHistoryAsync(Guid userId, BookingHistoryQuery query)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 10 : Math.Min(query.PageSize, MaxPageSize);

        if (!string.IsNullOrWhiteSpace(query.Status) && !IsValidStatus(query.Status))
        {
            throw new Exception("Invalid booking status filter.");
        }

        var (items, totalItems) = await _unitOfWork.BookingRepository.GetUserHistoryAsync(
            userId,
            query.Status,
            query.FromDate,
            query.ToDate,
            page,
            pageSize);

        return new PagedResult<BookingHistoryItemResponse>
        {
            Items = items.Select(ToBookingHistoryItemResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<BookingResponse> UpdateStatusAsync(Guid bookingId, string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new Exception("Status is required.");
        }

        var normalizedStatus = NormalizeStatus(status);
        if (normalizedStatus != BookingStatus.Confirmed && normalizedStatus != BookingStatus.Cancelled)
        {
            throw new Exception("Admin can only update status to Confirmed or Cancelled.");
        }

        var booking = await _unitOfWork.BookingRepository.GetByIdWithDetailsAsync(bookingId)
            ?? throw new Exception("Booking not found.");

        if (!string.Equals(booking.Status, BookingStatus.Pending, StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("Only Pending bookings can be updated.");
        }

        booking.Status = normalizedStatus;
        _unitOfWork.BookingRepository.Update(booking);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.BookingRepository.GetByIdWithDetailsAsync(bookingId)
            ?? throw new Exception("Cannot load updated booking.");

        return ToBookingResponse(updated);
    }

    public async Task<BookingResponse> CancelMyBookingAsync(Guid userId, Guid bookingId)
    {
        var booking = await _unitOfWork.BookingRepository.GetByIdWithDetailsAsync(bookingId)
            ?? throw new Exception("Booking not found.");

        if (booking.UserId != userId)
        {
            throw new Exception("You can only cancel your own bookings.");
        }

        if (!string.Equals(booking.Status, BookingStatus.Pending, StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("Only Pending bookings can be cancelled.");
        }

        booking.Status = BookingStatus.Cancelled;
        _unitOfWork.BookingRepository.Update(booking);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.BookingRepository.GetByIdWithDetailsAsync(bookingId)
            ?? throw new Exception("Cannot load updated booking.");

        return ToBookingResponse(updated);
    }

    private void ValidateCreateRequest(BookingCreateRequest request)
    {
        if (request.CourtId == Guid.Empty)
        {
            throw new Exception("CourtId is required.");
        }

        if (request.StartTime >= request.EndTime)
        {
            throw new Exception("EndTime must be greater than StartTime.");
        }

        if (request.StartTime.Date != request.EndTime.Date)
        {
            throw new Exception("Booking must be within the same day.");
        }
    }

    private void ValidateBookingTimeWindow(DateTime startTime, DateTime endTime, int slotMinutes)
    {
        var (openTime, closeTime, _) = ParseAndValidateBookingOptions();
        var day = startTime.Date;
        var opening = day.Add(openTime);
        var closing = day.Add(closeTime);

        if (startTime < opening || endTime > closing)
        {
            throw new Exception("Booking time is outside operating hours.");
        }

        var startOffset = startTime - opening;
        var endOffset = endTime - opening;
        if (startOffset.TotalMinutes % slotMinutes != 0 || endOffset.TotalMinutes % slotMinutes != 0 || startTime.Second != 0 || endTime.Second != 0)
        {
            throw new Exception($"Booking time must align with {slotMinutes}-minute slots.");
        }

        if ((endTime - startTime).TotalMinutes % slotMinutes != 0)
        {
            throw new Exception($"Booking duration must be a multiple of {slotMinutes} minutes.");
        }
    }

    private (TimeSpan OpenTime, TimeSpan CloseTime, int SlotMinutes) ParseAndValidateBookingOptions()
    {
        if (!TimeSpan.TryParse(_options.OpenTime, out var openTime))
        {
            throw new Exception("Invalid BookingOptions.OpenTime format.");
        }

        if (!TimeSpan.TryParse(_options.CloseTime, out var closeTime))
        {
            throw new Exception("Invalid BookingOptions.CloseTime format.");
        }

        if (closeTime <= openTime)
        {
            throw new Exception("BookingOptions.CloseTime must be greater than OpenTime.");
        }

        if (_options.SlotMinutes <= 0)
        {
            throw new Exception("BookingOptions.SlotMinutes must be greater than zero.");
        }

        return (openTime, closeTime, _options.SlotMinutes);
    }

    private static bool IsValidStatus(string status)
    {
        var normalized = NormalizeStatus(status);
        return normalized == BookingStatus.Pending
            || normalized == BookingStatus.Confirmed
            || normalized == BookingStatus.Cancelled;
    }

    private static string NormalizeStatus(string status)
    {
        if (string.Equals(status, BookingStatus.Pending, StringComparison.OrdinalIgnoreCase))
        {
            return BookingStatus.Pending;
        }

        if (string.Equals(status, BookingStatus.Confirmed, StringComparison.OrdinalIgnoreCase))
        {
            return BookingStatus.Confirmed;
        }

        if (string.Equals(status, BookingStatus.Cancelled, StringComparison.OrdinalIgnoreCase))
        {
            return BookingStatus.Cancelled;
        }

        return status.Trim();
    }

    private static BookingHistoryItemResponse ToBookingHistoryItemResponse(Booking booking)
    {
        return new BookingHistoryItemResponse
        {
            Id = booking.Id,
            CourtId = booking.CourtId,
            CourtName = booking.Court?.CourtName ?? string.Empty,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            TotalPrice = booking.TotalPrice ?? 0,
            Status = booking.Status,
            IsPaid = booking.IsPaid ?? false,
            CreatedAt = booking.CreatedAt
        };
    }

    private static BookingResponse ToBookingResponse(Booking booking)
    {
        return new BookingResponse
        {
            Id = booking.Id,
            UserId = booking.UserId,
            CourtId = booking.CourtId,
            CourtName = booking.Court?.CourtName ?? string.Empty,
            StartTime = booking.StartTime,
            EndTime = booking.EndTime,
            TotalPrice = booking.TotalPrice ?? 0,
            Status = booking.Status,
            IsPaid = booking.IsPaid ?? false,
            CreatedAt = booking.CreatedAt,
            Services = booking.BookingServices.Select(bs => new BookingServiceItemResponse
            {
                ServiceId = bs.ServiceId,
                ServiceName = bs.Service?.ServiceName ?? string.Empty,
                Quantity = bs.Quantity ?? 0,
                PriceAtBooking = bs.PriceAtBooking ?? 0
            }).ToList()
        };
    }
}
