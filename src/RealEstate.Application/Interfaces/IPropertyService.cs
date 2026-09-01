using RealEstate.Application.Common;
using RealEstate.Application.Features.Properties;

namespace RealEstate.Application.Interfaces;

public interface IPropertyService
{
    Task<Result<PagedResult<PropertyListItem>>> SearchAsync(PropertyFilterRequest request, CancellationToken ct);
    Task<Result<PropertyDetails>> GetByIdAsync(int id, CancellationToken ct);
    Task<Result<int>> CreateAsync(CreatePropertyRequest request, string userId, CancellationToken ct);
    Task<Result<bool>> UpdateAsync(int id, UpdatePropertyRequest request, string userId, bool isAdmin, CancellationToken ct);
    Task<Result<bool>> DeleteAsync(int id, string userId, bool isAdmin, CancellationToken ct);
    Task<Result<bool>> DeleteByAdminAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<PropertyListItem>> GetMineAsync(string userId, CancellationToken ct);
    Task<IReadOnlyList<PropertyListItem>> GetPendingAsync(CancellationToken ct);
    Task<Result<bool>> ApproveAsync(int id, ApprovePropertyRequest request, CancellationToken ct);
}
