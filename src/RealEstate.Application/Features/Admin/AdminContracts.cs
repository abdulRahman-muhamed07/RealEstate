using RealEstate.Domain.Entities;

namespace RealEstate.Application.Features.Admin;

public sealed record UserSummary(string Id,string Name,string Email,string? PhoneNumber,UserRole Role,DateTime CreatedAt);
public sealed record AdminDashboardDto(int TotalUsers,int TotalProperties,int PendingProperties,int TotalBookings,int TotalReviews);
