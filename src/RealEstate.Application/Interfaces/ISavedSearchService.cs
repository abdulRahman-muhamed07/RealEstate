using RealEstate.Application.Common;
using RealEstate.Application.Features.SavedSearches;

namespace RealEstate.Application.Interfaces;

public interface ISavedSearchService
{
    Task<Result<SavedSearchDto>> CreateAsync(CreateSavedSearchRequest request, string userId, CancellationToken ct);
    Task<IReadOnlyList<SavedSearchDto>> GetMineAsync(string userId, CancellationToken ct);
    Task<Result<bool>> DeleteAsync(int id, string userId, CancellationToken ct);
}
