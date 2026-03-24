using Application.Interfaces.IServices;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class AiService : IAiService
{
    private readonly IConfiguration _configuration;

    public AiService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<string> GenerateReplyAsync(string userMessage, Guid userId)
    {
        // Simple rule-based replies. Replace with external AI call if needed.
        var msg = (userMessage ?? string.Empty).ToLowerInvariant();

        if (msg.Contains("giá") || msg.Contains("price") || msg.Contains("bao nhiêu"))
            return Task.FromResult("Giá sản phẩm/báo giá sẽ được cung cấp chi tiết khi bạn gửi thông tin sản phẩm hoặc mã. Bạn cần mình hỗ trợ gì thêm không?");

        if (msg.Contains("đặt") || msg.Contains("đặt sân") || msg.Contains("book") || msg.Contains("đặt lịch"))
            return Task.FromResult("Để đặt sân, vui lòng cho biết ngày, giờ và số lượng giờ. Bạn muốn đặt khi nào?");

        if (msg.Contains("hủy") || msg.Contains("cancel"))
            return Task.FromResult("Bạn muốn hủy đặt sân/đơn hàng nào? Vui lòng cung cấp mã đặt/xác nhận để mình kiểm tra.");

        if (msg.Contains("mở cửa") || msg.Contains("giờ mở cửa"))
            return Task.FromResult("Giờ mở cửa: 06:00 - 22:00 mỗi ngày. Bạn cần giờ nào cụ thể?");

        // Fallback
        return Task.FromResult("Cảm ơn bạn đã liên hệ. Mình đã nhận yêu cầu và sẽ trả lời trong thời gian sớm nhất. Nếu bạn cần hỗ trợ gấp liên quan đến đặt sân hoặc đơn hàng, mình đã thông báo tới admin.");
    }
}
