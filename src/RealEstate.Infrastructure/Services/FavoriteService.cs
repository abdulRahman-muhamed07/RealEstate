using Microsoft.EntityFrameworkCore;
using RealEstate.Application.Common;
using RealEstate.Application.Features.Favorites;
using RealEstate.Application.Interfaces;
using RealEstate.Infrastructure.Persistence;

namespace RealEstate.Infrastructure.Services;

public sealed class FavoriteService(AppDbContext db) : IFavoriteService
{
    public async Task<IReadOnlyList<FavoriteDto>> GetMineAsync(string userId, CancellationToken ct) =>
        await db.Favorites.AsNoTracking().Where(x => x.UserId == userId).Include(x => x.Property).ThenInclude(x => x.Images)
            .Select(x => new FavoriteDto(x.PropertyId, x.Property.Title, x.Property.Price, x.Property.Images.Select(i => (string?)i.Url).FirstOrDefault(), x.Property.Location))
            .ToListAsync(ct);

    public async Task<Result<bool>> AddAsync(int propertyId, string userId, CancellationToken ct)
    {
        if (!await db.Properties.AnyAsync(x => x.Id == propertyId && x.IsApproved, ct))
            return Result<bool>.Fail(ErrorCode.NotFound, "Property not found.");
        if (await db.Favorites.AnyAsync(x => x.PropertyId == propertyId && x.UserId == userId, ct))
            return Result<bool>.Ok(true);
        db.Favorites.Add(new Domain.Entities.Favorite { PropertyId = propertyId, UserId = userId });
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException) { return Result<bool>.Ok(true); }
        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> RemoveAsync(int propertyId, string userId, CancellationToken ct)
    {
        var favorite = await db.Favorites.FirstOrDefaultAsync(x => x.PropertyId == propertyId && x.UserId == userId, ct);
        if (favorite is null) return Result<bool>.Fail(ErrorCode.NotFound, "Favorite not found.");
        db.Favorites.Remove(favorite);
        await db.SaveChangesAsync(ct);
        return Result<bool>.Ok(true);
    }
}
