using RealEstate.Application.Common;
using RealEstate.Application.Features.Favorites;

namespace RealEstate.Application.Interfaces;

public interface IFavoriteService
{
    Task<IReadOnlyList<FavoriteDto>> GetMineAsync(string userId, CancellationToken ct);
    Task<Result<bool>> AddAsync(int propertyId, string userId, CancellationToken ct);
    Task<Result<bool>> RemoveAsync(int propertyId, string userId, CancellationToken ct);
}
