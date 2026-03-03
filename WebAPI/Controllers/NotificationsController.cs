using Application.DTOs.RequestDTOs.Notification;
using Application.DTOs.ApiResponseDTO;
using Application.Interfaces.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(Guid userId)
    {
        var result = await _notificationService.GetByUserIdAsync(userId);
        return Ok(ApiResponse.Success("Get notifications successfully.", result));
    }

    [HttpPost("mark-as-read/{id}")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        try
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok(ApiResponse.Success("Notification marked as read."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] NotificationCreateRequest request)
    {
        try
        {
            await _notificationService.CreateAsync(request);
            return Ok(ApiResponse.Success("Notification created successfully."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }
}
