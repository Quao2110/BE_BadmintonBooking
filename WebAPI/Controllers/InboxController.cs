using System.Security.Claims;
using Domain.Entities;
using Infrastructure.DbContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/inbox")]
[ApiController]
[Authorize]
public class InboxController : ControllerBase
{
    private readonly BadmintonBooking_PRM393Context _db;

    public InboxController(BadmintonBooking_PRM393Context db)
    {
        _db = db;
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
