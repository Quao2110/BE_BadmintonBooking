using Application.DTOs.ApiResponseDTO;
using Application.DTOs.RequestDTOs.Order;
using Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace WebAPI.Controllers;

[Route("api/orders")]
[ApiController]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            throw new Exception("Unauthorized: Invalid user ID in token.");
        return userId;
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _orderService.CheckoutAsync(userId, request);
            return Ok(ApiResponse.Success("Order created successfully.", result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrderById([FromRoute] Guid id)
    {
        try
        {
            var result = await _orderService.GetOrderByIdAsync(id);
            return Ok(ApiResponse.Success("Order retrieved successfully.", result));
        }
        catch (Exception ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpGet("my-orders")]
    public async Task<IActionResult> GetMyOrders()
    {
        try
        {
            var userId = GetUserId();
            var result = await _orderService.GetOrdersByUserAsync(userId);
            return Ok(ApiResponse.Success("Orders retrieved successfully.", result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetOrdersPaged(
        [FromQuery] Guid? userId,
        [FromQuery] string? orderStatus,
        [FromQuery] string? paymentStatus,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _orderService.GetOrdersPagedAsync(
                userId,
                orderStatus,
                paymentStatus,
                page,
                pageSize);
            return Ok(ApiResponse.Success("Orders retrieved successfully.", result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateOrderStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateOrderStatusRequest request)
    {
        try
        {
            var result = await _orderService.UpdateOrderStatusAsync(id, request.NewStatus);
            return Ok(ApiResponse.Success("Order status updated successfully.", result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> CancelOrder([FromRoute] Guid id)
    {
        try
        {
            var result = await _orderService.CancelOrderAsync(id);
            return Ok(ApiResponse.Success("Order cancelled successfully.", result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }
}



