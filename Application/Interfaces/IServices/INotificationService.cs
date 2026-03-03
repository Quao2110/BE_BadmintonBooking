using Application.DTOs.RequestDTOs.Notification;
using Application.DTOs.ResponseDTOs.Notification;

namespace Application.Interfaces.IServices;

public interface INotificationService
{
    Task<IEnumerable<NotificationResponse>> GetByUserIdAsync(Guid userId);
    Task MarkAsReadAsync(Guid id);
    Task CreateAsync(NotificationCreateRequest request);
}
