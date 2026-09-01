using Microsoft.EntityFrameworkCore;
using RealEstate.Application.Contracts;
using RealEstate.Domain.Entities;
using RealEstate.Infrastructure.Persistence;

namespace RealEstate.Infrastructure.Services;

public sealed class PropertyService(AppDbContext db, IImageStorage images) : IPropertyService
{
    public async Task<PagedResult<PropertyListItem>> SearchAsync(PropertyFilterRequest f, string? currentUserId, CancellationToken ct)
    {
        var q = db.Properties.AsNoTracking().Where(p => p.IsApproved).Include(p => p.Category).Include(p => p.City).Include(p => p.Owner).Include(p => p.Images).AsQueryable();
        if (!string.IsNullOrWhiteSpace(f.Search)) q = q.Where(p => p.Title.Contains(f.Search) || p.Description.Contains(f.Search) || p.Location.Contains(f.Search));
        if (f.CityId.HasValue) q = q.Where(p => p.CityId == f.CityId);
        if (f.CategoryId.HasValue) q = q.Where(p => p.CategoryId == f.CategoryId);
        if (f.MinPrice.HasValue) q = q.Where(p => p.Price >= f.MinPrice);
        if (f.MaxPrice.HasValue) q = q.Where(p => p.Price <= f.MaxPrice);
        if (f.MinArea.HasValue) q = q.Where(p => p.Area >= f.MinArea);
        if (f.MaxArea.HasValue) q = q.Where(p => p.Area <= f.MaxArea);
        if (f.Bedrooms.HasValue) q = q.Where(p => p.Bedrooms >= f.Bedrooms);
        if (f.Bathrooms.HasValue) q = q.Where(p => p.Bathrooms >= f.Bathrooms);
        if (f.ListingType.HasValue) q = q.Where(p => p.ListingType == f.ListingType);
        if (f.Status.HasValue) q = q.Where(p => p.Status == f.Status);
        if (!string.IsNullOrWhiteSpace(f.Type)) q = q.Where(p => p.Type == f.Type);
        q = f.SortBy.ToLowerInvariant() switch { "price_asc" => q.OrderBy(p => p.Price), "price_desc" => q.OrderByDescending(p => p.Price), "area_asc" => q.OrderBy(p => p.Area), "area_desc" => q.OrderByDescending(p => p.Area), _ => q.OrderByDescending(p => p.CreatedAt) };
        var page = Math.Max(1, f.Page); var size = Math.Clamp(f.PageSize, 1, 100); var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * size).Take(size).Select(MapExpr()).ToListAsync(ct);
        return new PagedResult<PropertyListItem>(items, total, page, size, (int)Math.Ceiling(total / (double)size));
    }

    public async Task<PropertyDetails?> GetByIdAsync(int id, CancellationToken ct)
    {
        var p = await db.Properties.AsNoTracking().Include(x => x.Category).Include(x => x.City).Include(x => x.Owner).Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id && x.IsApproved, ct);
        if (p is null) return null;
        var ratings = await db.Reviews.Where(x => x.PropertyId == id).Select(x => (double?)x.Rating).AverageAsync(ct) ?? 0;
        var count = await db.Reviews.CountAsync(x => x.PropertyId == id, ct);
        return new PropertyDetails(Map(p), ratings, count);
    }

    public async Task<ContractsResult<int>> CreateAsync(CreatePropertyRequest r, string userId, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([userId], ct); if (user is null || user.Role is not (UserRole.Vendor or UserRole.Admin)) return ContractsResult<int>.Fail("Only vendors can list properties.", 403);
        if (!await db.Categories.AnyAsync(x => x.Id == r.CategoryId, ct)) return ContractsResult<int>.Fail("Category not found.", 400);
        var p = new Property { Title = r.Title.Trim(), Description = r.Description.Trim(), Price = r.Price, Area = r.Area, Bedrooms = r.Bedrooms, Bathrooms = r.Bathrooms, Type = r.Type.Trim().ToLowerInvariant(), ListingType = r.ListingType, Location = r.Location.Trim(), CategoryId = r.CategoryId, CityId = r.CityId, OwnerId = userId, IsApproved = user.Role == UserRole.Admin };
        db.Properties.Add(p); await db.SaveChangesAsync(ct);
        var urls = await images.SaveAsync(r.Images.Take(8), ct); foreach (var url in urls) p.Images.Add(new PropertyImage { Url = url, FileName = Path.GetFileName(url), PropertyId = p.Id }); await db.SaveChangesAsync(ct);
        return ContractsResult<int>.Ok(p.Id);
    }

    public async Task<ContractsResult<bool>> UpdateAsync(int id, UpdatePropertyRequest r, string userId, bool isAdmin, CancellationToken ct)
    {
        var p = await db.Properties.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id, ct); if (p is null) return ContractsResult<bool>.Fail("Property not found.", 404);
        if (!isAdmin && p.OwnerId != userId) return ContractsResult<bool>.Fail("Forbidden.", 403);
        p.Title = r.Title.Trim(); p.Description = r.Description.Trim(); p.Price = r.Price; p.Area = r.Area; p.Bedrooms = r.Bedrooms; p.Bathrooms = r.Bathrooms; p.Type = r.Type.Trim().ToLowerInvariant(); p.ListingType = r.ListingType; p.Location = r.Location.Trim(); p.CategoryId = r.CategoryId; p.CityId = r.CityId; p.IsApproved = isAdmin; p.Status = PropertyStatus.Available;
        if (r.Images.Count > 0) { await images.DeleteAsync(p.Images.Select(x => x.Url), ct); db.PropertyImages.RemoveRange(p.Images); foreach (var url in await images.SaveAsync(r.Images.Take(8), ct)) p.Images.Add(new PropertyImage { Url = url, FileName = Path.GetFileName(url) }); }
        await db.SaveChangesAsync(ct); return ContractsResult<bool>.Ok(true);
    }

    public async Task<ContractsResult<bool>> DeleteAsync(int id, string userId, bool isAdmin, CancellationToken ct)
    {
        var p = await db.Properties.Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id, ct); if (p is null) return ContractsResult<bool>.Fail("Property not found.", 404);
        if (!isAdmin && p.OwnerId != userId) return ContractsResult<bool>.Fail("Forbidden.", 403); await images.DeleteAsync(p.Images.Select(x => x.Url), ct); db.Properties.Remove(p); await db.SaveChangesAsync(ct); return ContractsResult<bool>.Ok(true);
    }
    public async Task<IReadOnlyList<PropertyListItem>> GetMineAsync(string userId, CancellationToken ct) => await db.Properties.AsNoTracking().Where(x => x.OwnerId == userId).Include(x => x.Category).Include(x => x.City).Include(x => x.Owner).Include(x => x.Images).OrderByDescending(x => x.CreatedAt).Select(MapExpr()).ToListAsync(ct);
    public async Task<IReadOnlyList<PropertyListItem>> GetPendingAsync(CancellationToken ct) => await db.Properties.AsNoTracking().Where(x => !x.IsApproved).Include(x => x.Category).Include(x => x.City).Include(x => x.Owner).Include(x => x.Images).OrderByDescending(x => x.CreatedAt).Select(MapExpr()).ToListAsync(ct);
    public async Task<ContractsResult<bool>> ApproveAsync(int id, ApprovePropertyRequest r, CancellationToken ct) { var p = await db.Properties.FindAsync([id], ct); if (p is null) return ContractsResult<bool>.Fail("Property not found.",404); if (!r.Approve) { db.Properties.Remove(p); await db.SaveChangesAsync(ct); return ContractsResult<bool>.Ok(true); } p.IsApproved = true; p.ListingType = r.ListingType ?? p.ListingType; p.Status = PropertyStatus.Available; await db.SaveChangesAsync(ct); return ContractsResult<bool>.Ok(true); }

    private static PropertyListItem Map(Property p) => new(p.Id,p.Title,p.Price,p.Area,p.Bedrooms,p.Bathrooms,p.Type,p.ListingType,p.Status,p.Location,p.CreatedAt,p.CategoryId,p.Category?.Name ?? "",p.CityId,p.City?.Name,p.OwnerId,$"{p.Owner?.FirstName} {p.Owner?.LastName}".Trim(),p.Images.Select(i=>i.Url).ToList());
    private static System.Linq.Expressions.Expression<Func<Property,PropertyListItem>> MapExpr() => p => new PropertyListItem(p.Id,p.Title,p.Price,p.Area,p.Bedrooms,p.Bathrooms,p.Type,p.ListingType,p.Status,p.Location,p.CreatedAt,p.CategoryId,p.Category.Name,p.CityId,p.City == null ? null : p.City.Name,p.OwnerId,(p.Owner.FirstName + " " + p.Owner.LastName).Trim(),p.Images.Select(i=>i.Url).ToList());
}
