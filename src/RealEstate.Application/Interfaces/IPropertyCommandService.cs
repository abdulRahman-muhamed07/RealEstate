using RealEstate.Application.Common;
using RealEstate.Application.Features.Properties;

namespace RealEstate.Application.Interfaces;

public interface IPropertyCommandService
{
    Task<Result<int>> CreateAsync(CreatePropertyRequest request, string userId, CancellationToken ct);
    Task<Result<bool>> UpdateAsync(int id, UpdatePropertyRequest request, string userId, bool isAdmin, CancellationToken ct);
    Task<Result<bool>> DeleteAsync(int id, string userId, bool isAdmin, CancellationToken ct);
    Task<Result<bool>> DeleteByAdminAsync(int id, CancellationToken ct);
    Task<Result<bool>> ApproveAsync(int id, ApprovePropertyRequest request, CancellationToken ct);
}