using RealEstate.Application.Features.Notifications;

namespace RealEstate.Application.Interfaces;

public interface INotificationService
{
    Task<IReadOnlyList<NotificationDto>> GetMineAsync(string userId, CancellationToken ct);
    Task<bool> MarkReadAsync(int id, string userId, CancellationToken ct);
}
