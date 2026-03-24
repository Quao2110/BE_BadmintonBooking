using Application.DTOs.ApiResponseDTO;
using Application.DTOs.RequestDTOs.Payment;
using Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers;

[Route("api/payments")]
[ApiController]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [Authorize]
    [HttpPost("vnpay/create-link")]
    public async Task<IActionResult> CreateVnPayLink([FromBody] CreateVnPayPaymentUrlRequest request)
    {
        try
        {
            var userId = GetUserId();
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _paymentService.CreateVnPayPaymentUrlAsync(userId, request, clientIp);
            return Ok(ApiResponse.Success("VNPAY payment link created successfully.", result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [Authorize]
    [HttpPost("vnpay/create-booking-link")]
    public async Task<IActionResult> CreateVnPayBookingLink([FromBody] CreateVnPayPaymentUrlRequest request)
    {
        try
        {
            var userId = GetUserId();
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _paymentService.CreateVnPayBookingPaymentUrlAsync(userId, request, clientIp);
            return Ok(ApiResponse.Success("VNPAY booking payment link created successfully.", result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [AllowAnonymous]
    [HttpGet("vnpay/ipn")]
    public async Task<IActionResult> VnPayIpn()
    {
        var queryParams = Request.Query
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString(), StringComparer.OrdinalIgnoreCase);

        var result = await _paymentService.HandleVnPayIpnAsync(queryParams);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("vnpay/return")]
    public async Task<IActionResult> VnPayReturn()
    {
        try
        {
            var queryParams = Request.Query
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString(), StringComparer.OrdinalIgnoreCase);

            var result = await _paymentService.HandleVnPayReturnAsync(queryParams);
            return Ok(ApiResponse.Success("Payment result received.", result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            throw new Exception("Unauthorized: Invalid user ID in token.");

        return userId;
    }
}
