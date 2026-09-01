using RealEstate.Application.Common;
using RealEstate.Application.Features.Reviews;

namespace RealEstate.Application.Interfaces;

public interface IReviewService
{
    Task<IReadOnlyList<ReviewDto>> GetForPropertyAsync(int propertyId, CancellationToken ct);
    Task<Result<ReviewDto>> CreateAsync(CreateReviewRequest request, string userId, CancellationToken ct);
    Task<Result<bool>> UpdateAsync(int id, UpdateReviewRequest request, string userId, CancellationToken ct);
    Task<Result<bool>> DeleteAsync(int id, string userId, bool isAdmin, CancellationToken ct);
    Task<IReadOnlyList<ReviewDto>> GetMineAsync(string userId, CancellationToken ct);
}
