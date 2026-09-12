using Microsoft.EntityFrameworkCore;
using RealEstate.Application.Common;
using RealEstate.Application.Features.SavedSearches;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Infrastructure.Persistence;

namespace RealEstate.Infrastructure.Services;

public sealed class SavedSearchService(AppDbContext db) : ISavedSearchService
{
    public async Task<Result<SavedSearchDto>> CreateAsync(CreateSavedSearchRequest request, string userId, CancellationToken ct)
    {
        if (request.MinPrice.HasValue && request.MaxPrice.HasValue && request.MinPrice > request.MaxPrice)
            return Result<SavedSearchDto>.Fail(ErrorCode.Validation, "Minimum price cannot exceed maximum price.");

        var search = new SavedSearch
        {
            UserId = userId,
            Name = request.Name.Trim(),
            CityId = request.CityId,
            CategoryId = request.CategoryId,
            MinPrice = request.MinPrice,
            MaxPrice = request.MaxPrice,
            Bedrooms = request.Bedrooms,
            ListingType = request.ListingType,
            Type = request.Type?.Trim().ToLowerInvariant()
        };
        db.SavedSearches.Add(search);
        await db.SaveChangesAsync(ct);
        return Result<SavedSearchDto>.Ok(Map(search));
    }

    public async Task<IReadOnlyList<SavedSearchDto>> GetMineAsync(string userId, CancellationToken ct) =>
        await db.SavedSearches.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new SavedSearchDto(x.Id, x.Name, x.CityId, x.CategoryId, x.MinPrice, x.MaxPrice, x.Bedrooms, x.ListingType, x.Type, x.IsActive))
            .ToListAsync(ct);

    public async Task<Result<bool>> DeleteAsync(int id, string userId, CancellationToken ct)
    {
        var search = await db.SavedSearches.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId, ct);
        if (search is null)
            return Result<bool>.Fail(ErrorCode.NotFound, "Saved search not found.");
        db.SavedSearches.Remove(search);
        await db.SaveChangesAsync(ct);
        return Result<bool>.Ok(true);
    }

    private static SavedSearchDto Map(SavedSearch search) =>
        new(search.Id, search.Name, search.CityId, search.CategoryId, search.MinPrice, search.MaxPrice, search.Bedrooms, search.ListingType, search.Type, search.IsActive);
}
