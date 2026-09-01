using RealEstate.Application.Common;
using RealEstate.Application.Features.Admin;
using RealEstate.Application.Features.Properties;

namespace RealEstate.Application.Interfaces;

public interface IAdminService
{
    Task<AdminDashboardDto> DashboardAsync(CancellationToken ct);
    Task<IReadOnlyList<UserSummary>> GetUsersAsync(CancellationToken ct);
    Task<Result<bool>> DeleteUserAsync(string userId, CancellationToken ct);
    Task<IReadOnlyList<PropertyListItem>> GetPropertiesAsync(CancellationToken ct);
}
