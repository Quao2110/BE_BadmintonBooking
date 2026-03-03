using Application.DTOs.RequestDTOs.Shop;
using Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShopsController : ControllerBase
{
    private readonly IShopService _shopService;

    public ShopsController(IShopService shopService)
    {
        _shopService = shopService;
    }

    [HttpGet]
    public async Task<IActionResult> GetShopInfo()
    {
        var result = await _shopService.GetShopInfoAsync();
        if (result == null) return NotFound("Shop info not configured.");
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateShopInfo(Guid id, [FromBody] ShopUpdateRequest request)
    {
        try
        {
            var result = await _shopService.UpdateShopInfoAsync(id, request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("distance")]
    public async Task<IActionResult> CalculateDistance([FromQuery] string userLat, [FromQuery] string userLng)
    {
        try
        {
            // Xử lý thay thế dấu phẩy thành dấu chấm và loại bỏ khoảng trắng
            var latStr = userLat.Replace(",", ".").Trim();
            var lngStr = userLng.Replace(",", ".").Trim();

            if (!double.TryParse(latStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lat) ||
                !double.TryParse(lngStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double lng))
            {
                return BadRequest("Tọa độ không hợp lệ. Vui lòng nhập số thực (VD: 10.827 hoặc 10,827).");
            }

            var result = await _shopService.CalculateDistanceAsync(lat, lng);
            return Ok(new { DistanceKm = result });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
