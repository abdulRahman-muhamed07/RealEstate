using RealEstate.Application.Common;
using RealEstate.Application.Features.Properties;

namespace RealEstate.Application.Interfaces;

public interface IPropertyQueryService
{
    Task<Result<PagedResult<PropertyListItem>>> SearchAsync(PropertyFilterRequest request, CancellationToken ct);
    Task<Result<PropertyDetails>> GetByIdAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<PropertyListItem>> GetMineAsync(string userId, CancellationToken ct);
    Task<IReadOnlyList<PropertyListItem>> GetPendingAsync(CancellationToken ct);
}