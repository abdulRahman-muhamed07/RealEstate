using Microsoft.EntityFrameworkCore;
using RealEstate.Application.Features.Notifications;
using RealEstate.Application.Interfaces;
using RealEstate.Infrastructure.Persistence;

namespace RealEstate.Infrastructure.Services;

public sealed class NotificationService(AppDbContext db) : INotificationService
{
    public async Task<IReadOnlyList<NotificationDto>> GetMineAsync(string userId, CancellationToken ct) =>
        await db.Notifications.AsNoTracking().Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new NotificationDto(x.Id, x.Title, x.Message, x.Type, x.IsRead, x.CreatedAt))
            .ToListAsync(ct);

    public async Task<bool> MarkReadAsync(int id, string userId, CancellationToken ct)
    {
        var notification = await db.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);
        if (notification is null) return false;
        notification.IsRead = true;
        await db.SaveChangesAsync(ct);
        return true;
    }
}
