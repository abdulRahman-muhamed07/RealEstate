using Microsoft.EntityFrameworkCore;
using RealEstate.Application.Common;
using RealEstate.Application.Features.Properties;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Infrastructure.Persistence;

namespace RealEstate.Infrastructure.Services;

public sealed class PropertyService(AppDbContext db, IImageStorage imageStorage) : IPropertyService
{
    public async Task<Result<PagedResult<PropertyListItem>>> SearchAsync(PropertyFilterRequest request, CancellationToken ct)
    {
        var query = db.Properties.AsNoTracking().Where(x => x.IsApproved);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.Title.Contains(search) || x.Description.Contains(search) || x.Location.Contains(search));
        }
        if (request.CityId.HasValue) query = query.Where(x => x.CityId == request.CityId);
        if (request.CategoryId.HasValue) query = query.Where(x => x.CategoryId == request.CategoryId);
        if (request.MinPrice.HasValue) query = query.Where(x => x.Price >= request.MinPrice.Value);
        if (request.MaxPrice.HasValue) query = query.Where(x => x.Price <= request.MaxPrice.Value);
        if (request.MinArea.HasValue) query = query.Where(x => x.Area >= request.MinArea.Value);
        if (request.MaxArea.HasValue) query = query.Where(x => x.Area <= request.MaxArea.Value);
        if (request.Bedrooms.HasValue) query = query.Where(x => x.Bedrooms >= request.Bedrooms.Value);
        if (request.Bathrooms.HasValue) query = query.Where(x => x.Bathrooms >= request.Bathrooms.Value);
        if (request.ListingType.HasValue) query = query.Where(x => x.ListingType == request.ListingType.Value);
        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status.Value);
        if (!string.IsNullOrWhiteSpace(request.Type)) query = query.Where(x => x.Type == request.Type.Trim().ToLower());

        query = request.SortBy.Trim().ToLowerInvariant() switch
        {
            "price_asc" => query.OrderBy(x => x.Price),
            "price_desc" => query.OrderByDescending(x => x.Price),
            "area_asc" => query.OrderBy(x => x.Area),
            "area_desc" => query.OrderByDescending(x => x.Area),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var total = await query.CountAsync(ct);
        var data = await query.Skip((page - 1) * pageSize).Take(pageSize).Select(MapExpression()).ToListAsync(ct);

        return Result<PagedResult<PropertyListItem>>.Ok(new PagedResult<PropertyListItem>(
            data, total, page, pageSize, (int)Math.Ceiling(total / (double)pageSize)));
    }

    public async Task<Result<PropertyDetails>> GetByIdAsync(int id, CancellationToken ct)
    {
        var property = await db.Properties.AsNoTracking()
            .Include(x => x.Category).Include(x => x.City).Include(x => x.Owner).Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsApproved, ct);

        if (property is null) return Result<PropertyDetails>.Fail(ErrorCode.NotFound, "Property not found.");

        var rating = await db.Reviews.Where(x => x.PropertyId == id).Select(x => (double?)x.Rating).AverageAsync(ct) ?? 0;
        var reviews = await db.Reviews.CountAsync(x => x.PropertyId == id, ct);
        return Result<PropertyDetails>.Ok(new PropertyDetails(Map(property), rating, reviews));
    }

    public async Task<Result<int>> CreateAsync(CreatePropertyRequest request, string userId, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([userId], ct);
        if (user is null) return Result<int>.Fail(ErrorCode.Unauthorized, "User not found.");
        if (user.Role is not (UserRole.Vendor or UserRole.Admin)) return Result<int>.Fail(ErrorCode.Forbidden, "Only vendors or admins can create properties.");
        if (!await db.Categories.AnyAsync(x => x.Id == request.CategoryId, ct)) return Result<int>.Fail(ErrorCode.Validation, "Category not found.");
        if (request.CityId.HasValue && !await db.Cities.AnyAsync(x => x.Id == request.CityId.Value, ct)) return Result<int>.Fail(ErrorCode.Validation, "City not found.");

        var property = new Property
        {
            Title = request.Title.Trim(), Description = request.Description.Trim(), Price = request.Price,
            Area = request.Area, Bedrooms = request.Bedrooms, Bathrooms = request.Bathrooms,
            Type = request.Type.Trim().ToLowerInvariant(), ListingType = request.ListingType,
            Location = request.Location.Trim(), CategoryId = request.CategoryId, CityId = request.CityId,
            OwnerId = userId, IsApproved = user.Role == UserRole.Admin,
            Status = PropertyStatus.Available
        };

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var savedUrls = Array.Empty<string>();
        try
        {
            db.Properties.Add(property);
            await db.SaveChangesAsync(ct);
            savedUrls = (await imageStorage.SaveAsync(request.Images.Take(8), ct)).ToArray();
            foreach (var url in savedUrls) property.Images.Add(new PropertyImage { PropertyId = property.Id, Url = url, FileName = Path.GetFileName(url) });
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return Result<int>.Ok(property.Id);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            await imageStorage.DeleteAsync(savedUrls, ct);
            throw;
        }
    }

    public async Task<Result<bool>> UpdateAsync(int id, UpdatePropertyRequest request, string userId, bool isAdmin, CancellationToken ct)
    {
        var property = await db.Properties.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (property is null) return Result<bool>.Fail(ErrorCode.NotFound, "Property not found.");
        if (!isAdmin && property.OwnerId != userId) return Result<bool>.Fail(ErrorCode.Forbidden, "You cannot update this property.");
        if (!await db.Categories.AnyAsync(x => x.Id == request.CategoryId, ct)) return Result<bool>.Fail(ErrorCode.Validation, "Category not found.");
        if (request.CityId.HasValue && !await db.Cities.AnyAsync(x => x.Id == request.CityId.Value, ct)) return Result<bool>.Fail(ErrorCode.Validation, "City not found.");

        var oldUrls = property.Images.Select(x => x.Url).ToArray();
        property.Title = request.Title.Trim(); property.Description = request.Description.Trim(); property.Price = request.Price;
        property.Area = request.Area; property.Bedrooms = request.Bedrooms; property.Bathrooms = request.Bathrooms;
        property.Type = request.Type.Trim().ToLowerInvariant(); property.ListingType = request.ListingType;
        property.Location = request.Location.Trim(); property.CategoryId = request.CategoryId; property.CityId = request.CityId;
        property.IsApproved = isAdmin; property.Status = PropertyStatus.Available;

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var newUrls = Array.Empty<string>();
        try
        {
            if (request.Images.Count > 0)
            {
                newUrls = (await imageStorage.SaveAsync(request.Images.Take(8), ct)).ToArray();
                db.PropertyImages.RemoveRange(property.Images);
                foreach (var url in newUrls) property.Images.Add(new PropertyImage { PropertyId = id, Url = url, FileName = Path.GetFileName(url) });
            }
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            if (newUrls.Length > 0) await imageStorage.DeleteAsync(oldUrls, ct);
            return Result<bool>.Ok(true);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            await imageStorage.DeleteAsync(newUrls, ct);
            throw;
        }
    }

    public async Task<Result<bool>> DeleteAsync(int id, string userId, bool isAdmin, CancellationToken ct)
    {
        var property = await db.Properties.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (property is null) return Result<bool>.Fail(ErrorCode.NotFound, "Property not found.");
        if (!isAdmin && property.OwnerId != userId) return Result<bool>.Fail(ErrorCode.Forbidden, "You cannot delete this property.");
        var urls = property.Images.Select(x => x.Url).ToArray();
        db.Properties.Remove(property);
        await db.SaveChangesAsync(ct);
        await imageStorage.DeleteAsync(urls, ct);
        return Result<bool>.Ok(true);
    }

    public async Task<IReadOnlyList<PropertyListItem>> GetMineAsync(string userId, CancellationToken ct) =>
        await db.Properties.AsNoTracking().Where(x => x.OwnerId == userId).Include(x => x.Category).Include(x => x.City).Include(x => x.Owner).Include(x => x.Images).OrderByDescending(x => x.CreatedAt).Select(MapExpression()).ToListAsync(ct);

    public async Task<IReadOnlyList<PropertyListItem>> GetPendingAsync(CancellationToken ct) =>
        await db.Properties.AsNoTracking().Where(x => !x.IsApproved).Include(x => x.Category).Include(x => x.City).Include(x => x.Owner).Include(x => x.Images).OrderByDescending(x => x.CreatedAt).Select(MapExpression()).ToListAsync(ct);

    public async Task<Result<bool>> ApproveAsync(int id, ApprovePropertyRequest request, CancellationToken ct)
    {
        var property = await db.Properties.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (property is null) return Result<bool>.Fail(ErrorCode.NotFound, "Property not found.");
        if (!request.Approve)
        {
            db.Properties.Remove(property);
        }
        else
        {
            property.IsApproved = true;
            if (request.ListingType.HasValue) property.ListingType = request.ListingType.Value;
            property.Status = PropertyStatus.Available;
        }
        await db.SaveChangesAsync(ct);
        return Result<bool>.Ok(true);
    }

    private static PropertyListItem Map(Property p) => new(
        p.Id, p.Title, p.Price, p.Area, p.Bedrooms, p.Bathrooms, p.Type, p.ListingType, p.Status,
        p.Location, p.CreatedAt, p.CategoryId, p.Category?.Name ?? string.Empty, p.CityId, p.City?.Name,
        p.OwnerId, $"{p.Owner?.FirstName} {p.Owner?.LastName}".Trim(), p.Images.Select(x => x.Url).ToList());

    private static System.Linq.Expressions.Expression<Func<Property, PropertyListItem>> MapExpression() => p =>
        new PropertyListItem(p.Id, p.Title, p.Price, p.Area, p.Bedrooms, p.Bathrooms, p.Type, p.ListingType,
            p.Status, p.Location, p.CreatedAt, p.CategoryId, p.Category.Name, p.CityId,
            p.City == null ? null : p.City.Name, p.OwnerId, (p.Owner.FirstName + " " + p.Owner.LastName).Trim(),
            p.Images.Select(i => i.Url).ToList());
}
