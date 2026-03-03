using Domain.Entities;
using Infrastructure.DbContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers;

[Route("api/admin/inbox")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminInboxController : ControllerBase
{
    private readonly BadmintonBooking_PRM393Context _db;

    public AdminInboxController(BadmintonBooking_PRM393Context db)
    {
        _db = db;
    }

    public record ReplyRequest(Guid ChatRoomId, string MessageText, string ImageUrl);

    [HttpGet("rooms")]
    public IActionResult GetActiveRooms()
    {
        var rooms = _db.ChatRooms
            .Where(r => r.ChatMessages.Any())
            .Select(r => new
            {
                r.Id,
                r.UserId,
                r.SupportId,
                r.LastMessage,
                r.UpdatedAt,
                UnreadCount = 0 // placeholder, can be implemented later
            })
            .OrderByDescending(r => r.UpdatedAt)
            .ToList();

        return Ok(new { rooms });
    }

    [HttpPost("reply")]
    public async Task<IActionResult> Reply([FromBody] ReplyRequest request)
    {
        try
        {
            var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (adminIdClaim == null) return Unauthorized();
            var adminId = Guid.Parse(adminIdClaim);

            var room = _db.ChatRooms.FirstOrDefault(r => r.Id == request.ChatRoomId);
            if (room == null) return NotFound(new { success = false, error = "Chat room not found" });

            // set support if not set
            if (room.SupportId == null) room.SupportId = adminId;

            var msg = new ChatMessage
            {
                ChatRoomId = room.Id,
                SenderId = adminId,
                MessageText = request.MessageText ?? string.Empty,
                ImageUrl = request.ImageUrl ?? string.Empty,
                SentAt = DateTime.UtcNow
            };
            _db.ChatMessages.Add(msg);

            room.LastMessage = request.MessageText;
            room.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(new { success = true, messageId = msg.Id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }
}
