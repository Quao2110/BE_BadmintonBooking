using System.Security.Claims;
using Domain.Entities;
using Infrastructure.DbContexts;
using Application.Interfaces.IServices;
using Microsoft.Extensions.Options;
using Application.Options;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/inbox")]
[ApiController]
[Authorize]
public class InboxController : ControllerBase
{
    private readonly BadmintonBooking_PRM393Context _db;
    private readonly IAiService _aiService;
    private readonly AiOptions _aiOptions;

    public InboxController(BadmintonBooking_PRM393Context db, IAiService aiService, IOptions<AiOptions> aiOptions)
    {
        _db = db;
        _aiService = aiService;
        _aiOptions = aiOptions.Value;
    }

    public record SendMessageRequest(string MessageText, string ImageUrl);

    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
            var userId = Guid.Parse(userIdClaim);

            // find or create chatroom for this user
            var chatRoom = _db.ChatRooms.FirstOrDefault(cr => cr.UserId == userId);
            if (chatRoom == null)
            {
                chatRoom = new ChatRoom
                {
                    UserId = userId,
                    LastMessage = request.MessageText,
                    UpdatedAt = DateTime.UtcNow
                };
                _db.ChatRooms.Add(chatRoom);
                await _db.SaveChangesAsync();
            }

            var msg = new ChatMessage
            {
                ChatRoomId = chatRoom.Id,
                SenderId = userId,
                MessageText = request.MessageText ?? string.Empty,
                ImageUrl = request.ImageUrl ?? string.Empty,
                SentAt = DateTime.UtcNow
            };
            _db.ChatMessages.Add(msg);

            chatRoom.LastMessage = request.MessageText;
            chatRoom.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // Auto-reply and admin notification logic
            // Define keywords related to booking/order
            var keywords = new[] { "đặt", "đặt sân", "đặt hàng", "đặt lịch", "order", "book", "mua", "hủy", "thanh toán" };
            var textLower = (request.MessageText ?? string.Empty).ToLowerInvariant();
            bool isBookingRelated = keywords.Any(k => textLower.Contains(k));

            // find admin users
            var admins = _db.Users.Where(u => u.Role == "Admin" && (u.IsActive == null || u.IsActive == true)).ToList();

            // If message related to booking/order, create notifications for admins
            if (isBookingRelated && admins.Any())
            {
                foreach (var admin in admins)
                {
                    var note = new Notification
                    {
                        UserId = admin.Id,
                        Title = "Khách có yêu cầu liên quan đến đặt sân/đơn hàng",
                        Message = $"Người dùng {userId} gửi: {request.MessageText}",
                        Type = "Inbox",
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    _db.Notifications.Add(note);
                }
            }

            // Determine whether AI auto-reply should be used
            // For testing: if EnableAutoReply is true, call AI reply immediately (bypass topic/time checks)
            if (_aiOptions.EnableAutoReply)
            {
                var aiReply = await _aiService.GenerateReplyAsync(request.MessageText ?? string.Empty, userId);
                if (!string.IsNullOrWhiteSpace(aiReply))
                {
                    // Prefer system bot user as sender for AI replies
                    var systemBot = _db.Users.FirstOrDefault(u => u.Role == "System");
                    var senderId = systemBot != null ? systemBot.Id : (admins.Any() ? admins.First().Id : userId);

                    var autoReply = new ChatMessage
                    {
                        ChatRoomId = chatRoom.Id,
                        SenderId = senderId,
                        MessageText = aiReply,
                        ImageUrl = string.Empty,
                        SentAt = DateTime.UtcNow
                    };
                    _db.ChatMessages.Add(autoReply);

                    chatRoom.LastMessage = aiReply;
                    chatRoom.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _db.SaveChangesAsync();

            return Ok(new { success = true, messageId = msg.Id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    [HttpGet("messages")]
    public IActionResult GetMessages()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized();
        var userId = Guid.Parse(userIdClaim);

        var chatRoom = _db.ChatRooms.FirstOrDefault(cr => cr.UserId == userId);
        if (chatRoom == null) return Ok(new { messages = Array.Empty<object>() });

        var messages = _db.ChatMessages
            .Where(m => m.ChatRoomId == chatRoom.Id)
            .OrderBy(m => m.SentAt)
            .Select(m => new
            {
                m.Id,
                m.SenderId,
                m.MessageText,
                m.ImageUrl,
                m.SentAt
            })
            .ToList();

        return Ok(new { messages });
    }
}
