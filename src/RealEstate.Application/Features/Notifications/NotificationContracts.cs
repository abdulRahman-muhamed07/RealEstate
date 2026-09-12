namespace RealEstate.Application.Features.Notifications;

public sealed record NotificationDto(int Id, string Title, string Message, string? Type, bool IsRead, DateTime CreatedAt);
