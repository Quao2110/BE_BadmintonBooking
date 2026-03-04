using System.Security.Claims;
using Application.DTOs.ApiResponseDTO;
using Application.DTOs.RequestDTOs.Booking;
using Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet("availability")]
    public async Task<IActionResult> GetAvailability([FromQuery] Guid courtId, [FromQuery] DateTime date)
    {
        try
        {
            var result = await _bookingService.GetAvailabilityAsync(courtId, date);
            return Ok(ApiResponse.Success("Availability retrieved successfully.", result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] BookingCreateRequest request)
    {
        try
        {
            var userId = GetUserIdOrThrow();
            var result = await _bookingService.CreateAsync(userId, request);
            return Ok(ApiResponse.Success("Booking created successfully.", result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpGet("my-history")]
    [Authorize]
    public async Task<IActionResult> GetMyHistory([FromQuery] BookingHistoryQuery query)
    {
        try
        {
            var userId = GetUserIdOrThrow();
            var result = await _bookingService.GetMyHistoryAsync(userId, query);
            return Ok(ApiResponse.Success("Booking history retrieved successfully.", result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus([FromRoute] Guid id, [FromBody] BookingStatusUpdateRequest request)
    {
        try
        {
            var result = await _bookingService.UpdateStatusAsync(id, request.Status);
            return Ok(ApiResponse.Success("Booking status updated successfully.", result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPatch("{id:guid}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelMyBooking([FromRoute] Guid id)
    {
        try
        {
            var userId = GetUserIdOrThrow();
            var result = await _bookingService.CancelMyBookingAsync(userId, id);
            return Ok(ApiResponse.Success("Booking cancelled successfully.", result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    private Guid GetUserIdOrThrow()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new Exception("Invalid user token.");
        }

        return userId;
    }
}
