using Microsoft.EntityFrameworkCore;
using RealEstate.Application.Common;
using RealEstate.Application.Features.Admin;
using RealEstate.Application.Features.Properties;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Infrastructure.Persistence;

namespace RealEstate.Infrastructure.Services;

public sealed class AdminService(AppDbContext db) : IAdminService
{
    public async Task<AdminDashboardDto> DashboardAsync(CancellationToken ct)
    {
        return new AdminDashboardDto(
            await db.Users.CountAsync(ct),
            await db.Properties.CountAsync(ct),
            await db.Properties.CountAsync(x => !x.IsApproved, ct),
            await db.Bookings.CountAsync(ct),
            await db.Reviews.CountAsync(ct));
    }

    public async Task<IReadOnlyList<UserSummary>> GetUsersAsync(CancellationToken ct)
    {
        return await db.Users
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new UserSummary(
                x.Id,
                (x.FirstName + " " + x.LastName).Trim(),
                x.Email,
                x.PhoneNumber,
                x.Role,
                x.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<Result<bool>> DeleteUserAsync(string userId, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([userId], ct);
        if (user is null)
            return Result<bool>.Fail(ErrorCode.NotFound, "User not found.");

        if (user.Role == UserRole.Admin)
            return Result<bool>.Fail(ErrorCode.InvalidOperation, "An admin account cannot be deleted.");

        var ownsProperties = await db.Properties.AnyAsync(x => x.OwnerId == userId, ct);
        if (ownsProperties)
        {
            return Result<bool>.Fail(
                ErrorCode.Conflict,
                "This user owns properties and cannot be deleted until those properties are removed.");
        }

        db.Users.Remove(user);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return Result<bool>.Fail(
                ErrorCode.Conflict,
                "The user cannot be deleted because related records still exist.");
        }

        return Result<bool>.Ok(true);
    }

    public async Task<IReadOnlyList<PropertyListItem>> GetPropertiesAsync(CancellationToken ct)
    {
        return await db.Properties
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new PropertyListItem(
                x.Id,
                x.Title,
                x.Price,
                x.Area,
                x.Bedrooms,
                x.Bathrooms,
                x.Type,
                x.ListingType,
                x.Status,
                x.Location,
                x.CreatedAt,
                x.CategoryId,
                x.Category.Name,
                x.CityId,
                x.City == null ? null : x.City.Name,
                x.OwnerId,
                (x.Owner.FirstName + " " + x.Owner.LastName).Trim(),
                x.Images.Select(image => image.Url).ToList()))
            .ToListAsync(ct);
    }
}
