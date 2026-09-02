using Microsoft.EntityFrameworkCore;
using RealEstate.Application.Common;
using RealEstate.Application.Features.Properties;
using RealEstate.Application.Interfaces;
using RealEstate.Infrastructure.Persistence;
using RealEstate.Infrastructure.Persistence.Mappings;

namespace RealEstate.Infrastructure.Services;

public sealed class PropertyQueryService(AppDbContext db) : IPropertyQueryService
{
    public async Task<Result<PagedResult<PropertyListItem>>> SearchAsync(PropertyFilterRequest request, CancellationToken ct)
    {
        var query = db.Properties.AsNoTracking().Where(x => x.IsApproved);
        query = ApplyFilters(query, request);
        query = ApplySorting(query, request.SortBy);

        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var totalCount = await query.CountAsync(ct);
        var properties = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(PropertyMapping.Projection())
            .ToListAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return Result<PagedResult<PropertyListItem>>.Ok(
            new PagedResult<PropertyListItem>(properties, totalCount, page, pageSize, totalPages));
    }

    public async Task<Result<PropertyDetails>> GetByIdAsync(int id, CancellationToken ct)
    {
        var property = await db.Properties.AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.City)
            .Include(x => x.Owner)
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsApproved, ct);

        if (property is null)
            return Result<PropertyDetails>.Fail(ErrorCode.NotFound, "Property not found.");

        var averageRating = await db.Reviews
            .Where(x => x.PropertyId == id)
            .Select(x => (double?)x.Rating)
            .AverageAsync(ct) ?? 0;
        var reviewCount = await db.Reviews.CountAsync(x => x.PropertyId == id, ct);

        return Result<PropertyDetails>.Ok(new PropertyDetails(PropertyMapping.Map(property), averageRating, reviewCount));
    }

    public async Task<IReadOnlyList<PropertyListItem>> GetMineAsync(string userId, CancellationToken ct) =>
        await db.Properties.AsNoTracking()
            .Where(x => x.OwnerId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(PropertyMapping.Projection())
            .ToListAsync(ct);

    public async Task<IReadOnlyList<PropertyListItem>> GetPendingAsync(CancellationToken ct) =>
        await db.Properties.AsNoTracking()
            .Where(x => !x.IsApproved)
            .OrderByDescending(x => x.CreatedAt)
            .Select(PropertyMapping.Projection())
            .ToListAsync(ct);

    private static IQueryable<Domain.Entities.Property> ApplyFilters(
        IQueryable<Domain.Entities.Property> query,
        PropertyFilterRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.Title.Contains(search) || x.Description.Contains(search) || x.Location.Contains(search));
        }
        if (request.CityId.HasValue) query = query.Where(x => x.CityId == request.CityId.Value);
        if (request.CategoryId.HasValue) query = query.Where(x => x.CategoryId == request.CategoryId.Value);
        if (request.MinPrice.HasValue) query = query.Where(x => x.Price >= request.MinPrice.Value);
        if (request.MaxPrice.HasValue) query = query.Where(x => x.Price <= request.MaxPrice.Value);
        if (request.MinArea.HasValue) query = query.Where(x => x.Area >= request.MinArea.Value);
        if (request.MaxArea.HasValue) query = query.Where(x => x.Area <= request.MaxArea.Value);
        if (request.Bedrooms.HasValue) query = query.Where(x => x.Bedrooms >= request.Bedrooms.Value);
        if (request.Bathrooms.HasValue) query = query.Where(x => x.Bathrooms >= request.Bathrooms.Value);
        if (request.ListingType.HasValue) query = query.Where(x => x.ListingType == request.ListingType.Value);
        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status.Value);
        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            var type = request.Type.Trim().ToLowerInvariant();
            query = query.Where(x => x.Type == type);
        }
        return query;
    }

    private static IQueryable<Domain.Entities.Property> ApplySorting(
        IQueryable<Domain.Entities.Property> query,
        string sortBy) => sortBy.Trim().ToLowerInvariant() switch
    {
        "price_asc" => query.OrderBy(x => x.Price).ThenByDescending(x => x.CreatedAt),
        "price_desc" => query.OrderByDescending(x => x.Price).ThenByDescending(x => x.CreatedAt),
        "area_asc" => query.OrderBy(x => x.Area).ThenByDescending(x => x.CreatedAt),
        "area_desc" => query.OrderByDescending(x => x.Area).ThenByDescending(x => x.CreatedAt),
        _ => query.OrderByDescending(x => x.CreatedAt)
    };
}
