using RealEstate.Application.Common;
using RealEstate.Application.Features.Admin;
using RealEstate.Application.Features.Auth;
using RealEstate.Application.Features.Bookings;
using RealEstate.Application.Features.Favorites;
using RealEstate.Application.Features.Properties;
using RealEstate.Application.Features.Reviews;
using RealEstate.Domain.Entities;

namespace RealEstate.Application.Interfaces;

public interface IPropertyService
{
    Task<Result<PagedResult<PropertyListItem>>> SearchAsync(PropertyFilterRequest request, CancellationToken ct);
    Task<Result<PropertyDetails>> GetByIdAsync(int id, CancellationToken ct);
    Task<Result<int>> CreateAsync(CreatePropertyRequest request, string userId, CancellationToken ct);
    Task<Result<bool>> UpdateAsync(int id, UpdatePropertyRequest request, string userId, bool isAdmin, CancellationToken ct);
    Task<Result<bool>> DeleteAsync(int id, string userId, bool isAdmin, CancellationToken ct);
    Task<IReadOnlyList<PropertyListItem>> GetMineAsync(string userId, CancellationToken ct);
    Task<IReadOnlyList<PropertyListItem>> GetPendingAsync(CancellationToken ct);
    Task<Result<bool>> ApproveAsync(int id, ApprovePropertyRequest request, CancellationToken ct);
}

public interface IBookingService
{
    Task<Result<BookingDto>> CreateAsync(int propertyId, string userId, CancellationToken ct);
    Task<IReadOnlyList<BookingDto>> GetMyAsync(string userId, CancellationToken ct);
    Task<IReadOnlyList<BookingDto>> GetVendorAsync(string vendorId, CancellationToken ct);
    Task<Result<bool>> ChangeStatusAsync(int id, BookingStatus status, string userId, bool isVendor, CancellationToken ct);
}

public interface IFavoriteService
{
    Task<IReadOnlyList<FavoriteDto>> GetMineAsync(string userId, CancellationToken ct);
    Task<Result<bool>> AddAsync(int propertyId, string userId, CancellationToken ct);
    Task<Result<bool>> RemoveAsync(int propertyId, string userId, CancellationToken ct);
}

public interface IReviewService
{
    Task<IReadOnlyList<ReviewDto>> GetForPropertyAsync(int propertyId, CancellationToken ct);
    Task<Result<ReviewDto>> CreateAsync(CreateReviewRequest request, string userId, CancellationToken ct);
    Task<Result<bool>> UpdateAsync(int id, UpdateReviewRequest request, string userId, CancellationToken ct);
    Task<Result<bool>> DeleteAsync(int id, string userId, bool isAdmin, CancellationToken ct);
    Task<IReadOnlyList<ReviewDto>> GetMineAsync(string userId, CancellationToken ct);
}

public interface IAdminService
{
    Task<AdminDashboardDto> DashboardAsync(CancellationToken ct);
    Task<IReadOnlyList<UserSummary>> GetUsersAsync(CancellationToken ct);
    Task<Result<bool>> DeleteUserAsync(string userId, CancellationToken ct);
    Task<IReadOnlyList<PropertyListItem>> GetPropertiesAsync(CancellationToken ct);
}

public interface ITokenService { string CreateToken(User user); }
public interface IImageStorage { Task<IReadOnlyList<string>> SaveAsync(IEnumerable<UploadedFile> files, CancellationToken ct); Task DeleteAsync(IEnumerable<string> urls, CancellationToken ct); }
