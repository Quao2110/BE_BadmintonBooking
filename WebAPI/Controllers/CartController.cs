using Application.DTOs.ApiResponseDTO;
using Application.DTOs.RequestDTOs.Cart;
using Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers;

[Route("api/cart")]
[ApiController]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            throw new Exception("Unauthorized: Invalid user ID in token.");
        return userId;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        try
        {
            var userId = GetUserId();
            var result = await _cartService.GetCartAsync(userId);
            return Ok(ApiResponse.Success("Cart retrieved successfully.", result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _cartService.AddToCartAsync(userId, request);
            return Ok(ApiResponse.Success("Product added to cart successfully.", result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPut("item/{cartItemId:guid}")]
    public async Task<IActionResult> UpdateCartItem(
        [FromRoute] Guid cartItemId,
        [FromBody] UpdateCartItemRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _cartService.UpdateCartItemAsync(userId, cartItemId, request);
            return Ok(ApiResponse.Success("Cart item updated successfully.", result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpDelete("item/{cartItemId:guid}")]
    public async Task<IActionResult> DeleteCartItem([FromRoute] Guid cartItemId)
    {
        try
        {
            var userId = GetUserId();
            await _cartService.DeleteCartItemAsync(userId, cartItemId);
            return Ok(ApiResponse.Success("Cart item deleted successfully."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpDelete("clear")]
    public async Task<IActionResult> ClearCart()
    {
        try
        {
            var userId = GetUserId();
            await _cartService.ClearCartAsync(userId);
            return Ok(ApiResponse.Success("Cart cleared successfully."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }
}

