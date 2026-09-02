using Microsoft.EntityFrameworkCore;
using RealEstate.Application.Common;
using RealEstate.Application.Features.Properties;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Enums;
using RealEstate.Infrastructure.Persistence;

namespace RealEstate.Infrastructure.Services;

public sealed class PropertyCommandService(AppDbContext db, IImageStorage imageStorage) : IPropertyCommandService
{
    public async Task<Result<int>> CreateAsync(CreatePropertyRequest request, string userId, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([userId], ct);
        if (user is null)
            return Result<int>.Fail(ErrorCode.Unauthorized, "User not found.");
        if (user.Role is not (UserRole.Vendor or UserRole.Admin))
            return Result<int>.Fail(ErrorCode.Forbidden, "Only vendors or admins can create properties.");

        var validation = await ValidateReferencesAsync(request, ct);
        if (validation is not null)
            return Result<int>.Fail(ErrorCode.Validation, validation);

        var property = new Property
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Price = request.Price,
            Area = request.Area,
            Bedrooms = request.Bedrooms,
            Bathrooms = request.Bathrooms,
            Type = request.Type.Trim().ToLowerInvariant(),
            ListingType = request.ListingType,
            Location = request.Location.Trim(),
            CategoryId = request.CategoryId,
            CityId = request.CityId,
            OwnerId = userId,
            IsApproved = user.Role == UserRole.Admin
        };

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var savedUrls = Array.Empty<string>();
        try
        {
            db.Properties.Add(property);
            await db.SaveChangesAsync(ct);

            savedUrls = (await imageStorage.SaveAsync(request.Images.Take(8), ct)).ToArray();
            AddImages(property, savedUrls);
            await db.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);
            return Result<int>.Ok(property.Id);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            await imageStorage.DeleteAsync(savedUrls, CancellationToken.None);
            throw;
        }
    }

    public async Task<Result<bool>> UpdateAsync(int id, UpdatePropertyRequest request, string userId, bool isAdmin, CancellationToken ct)
    {
        var property = await db.Properties.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (property is null)
            return Result<bool>.Fail(ErrorCode.NotFound, "Property not found.");
        if (!isAdmin && property.OwnerId != userId)
            return Result<bool>.Fail(ErrorCode.Forbidden, "You cannot update this property.");

        var validation = await ValidateReferencesAsync(request, ct);
        if (validation is not null)
            return Result<bool>.Fail(ErrorCode.Validation, validation);

        var oldUrls = property.Images.Select(x => x.Url).ToArray();
        property.UpdateDetails(
            request.Title,
            request.Description,
            request.Price,
            request.Area,
            request.Bedrooms,
            request.Bathrooms,
            request.Type,
            request.ListingType,
            request.Location,
            request.CategoryId,
            request.CityId,
            isAdmin);

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var newUrls = Array.Empty<string>();
        try
        {
            if (request.Images.Count > 0)
            {
                newUrls = (await imageStorage.SaveAsync(request.Images.Take(8), ct)).ToArray();
                db.PropertyImages.RemoveRange(property.Images);
                AddImages(property, newUrls);
            }

            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            if (newUrls.Length > 0)
                await imageStorage.DeleteAsync(oldUrls, CancellationToken.None);

            return Result<bool>.Ok(true);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            await imageStorage.DeleteAsync(newUrls, CancellationToken.None);
            throw;
        }
    }

    public async Task<Result<bool>> DeleteAsync(int id, string userId, bool isAdmin, CancellationToken ct)
    {
        var property = await db.Properties.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (property is null)
            return Result<bool>.Fail(ErrorCode.NotFound, "Property not found.");
        if (!isAdmin && property.OwnerId != userId)
            return Result<bool>.Fail(ErrorCode.Forbidden, "You cannot delete this property.");
        return await DeletePropertyAsync(property, ct);
    }

    public async Task<Result<bool>> DeleteByAdminAsync(int id, CancellationToken ct)
    {
        var property = await db.Properties.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (property is null)
            return Result<bool>.Fail(ErrorCode.NotFound, "Property not found.");
        return await DeletePropertyAsync(property, ct);
    }

    public async Task<Result<bool>> ApproveAsync(int id, ApprovePropertyRequest request, CancellationToken ct)
    {
        var property = await db.Properties.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (property is null)
            return Result<bool>.Fail(ErrorCode.NotFound, "Property not found.");

        if (!request.Approve)
        {
            var urls = property.Images.Select(x => x.Url).ToArray();
            db.Properties.Remove(property);
            await db.SaveChangesAsync(ct);
            await imageStorage.DeleteAsync(urls, CancellationToken.None);
            return Result<bool>.Ok(true);
        }

        property.Approve(request.ListingType);
        await db.SaveChangesAsync(ct);
        return Result<bool>.Ok(true);
    }

    private async Task<string?> ValidateReferencesAsync(CreatePropertyRequest request, CancellationToken ct)
    {
        if (!await db.Categories.AnyAsync(x => x.Id == request.CategoryId, ct))
            return "Category not found.";
        if (request.CityId.HasValue && !await db.Cities.AnyAsync(x => x.Id == request.CityId.Value, ct))
            return "City not found.";
        return null;
    }

    private static void AddImages(Property property, IEnumerable<string> urls)
    {
        foreach (var url in urls)
            property.Images.Add(new PropertyImage { PropertyId = property.Id, Url = url, FileName = Path.GetFileName(url) });
    }

    private async Task<Result<bool>> DeletePropertyAsync(Property property, CancellationToken ct)
    {
        var urls = property.Images.Select(x => x.Url).ToArray();
        db.Properties.Remove(property);
        await db.SaveChangesAsync(ct);
        await imageStorage.DeleteAsync(urls, CancellationToken.None);
        return Result<bool>.Ok(true);
    }
}
