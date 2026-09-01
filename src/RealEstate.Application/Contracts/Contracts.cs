using Microsoft.AspNetCore.Http;
using RealEstate.Domain.Entities;

namespace RealEstate.Application.Contracts;

public record RegisterRequest(string FirstName, string LastName, string Email, string Password, string? PhoneNumber, UserRole Role);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, string UserId, string FirstName, string LastName, string Email, UserRole Role);

public sealed class PropertyFilterRequest
{
    public string? Search { get; init; }
    public int? CityId { get; init; }
    public int? CategoryId { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public double? MinArea { get; init; }
    public double? MaxArea { get; init; }
    public int? Bedrooms { get; init; }
    public int? Bathrooms { get; init; }
    public ListingType? ListingType { get; init; }
    public PropertyStatus? Status { get; init; }
    public string? Type { get; init; }
    public string SortBy { get; init; } = "newest";
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public sealed class CreatePropertyRequest
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public double Area { get; init; }
    public int Bedrooms { get; init; }
    public int Bathrooms { get; init; }
    public string Type { get; init; } = "apartment";
    public ListingType ListingType { get; init; } = ListingType.Sale;
    public string Location { get; init; } = string.Empty;
    public int CategoryId { get; init; }
    public int? CityId { get; init; }
    public List<IFormFile> Images { get; init; } = new();
}
public sealed class UpdatePropertyRequest : CreatePropertyRequest { }
public record ChangeBookingStatusRequest(BookingStatus Status);
public record CreateReviewRequest(int PropertyId, int Rating, string Comment);
public record UpdateReviewRequest(int Rating, string Comment);
public record ApprovePropertyRequest(bool Approve, ListingType? ListingType);

public record PropertyListItem(int Id, string Title, decimal Price, double Area, int Bedrooms, int Bathrooms, string Type, ListingType ListingType, PropertyStatus Status, string Location, DateTime CreatedAt, int CategoryId, string CategoryName, int? CityId, string? CityName, string OwnerId, string OwnerName, List<string> Images);
public record PropertyDetails(PropertyListItem Property, double AverageRating, int ReviewCount);
public record PagedResult<T>(IReadOnlyList<T> Data, int TotalCount, int Page, int PageSize, int TotalPages);
public record ReviewDto(int Id, string UserId, string UserName, int Rating, string Comment, DateTime CreatedAt);
public record BookingDto(int Id, int PropertyId, string PropertyTitle, string UserId, string UserName, DateTime BookingDate, BookingStatus Status, DateTime CreatedAt);
public record FavoriteDto(int PropertyId, string Title, decimal Price, string? ImageUrl, string Location);
public record AdminDashboardDto(int TotalUsers, int TotalProperties, int PendingProperties, int TotalBookings, int TotalReviews);

public interface IAuthService
{
    Task<ContractsResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct);
    Task<ContractsResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct);
}
public interface IPropertyService
{
    Task<PagedResult<PropertyListItem>> SearchAsync(PropertyFilterRequest request, string? currentUserId, CancellationToken ct);
    Task<PropertyDetails?> GetByIdAsync(int id, CancellationToken ct);
    Task<ContractsResult<int>> CreateAsync(CreatePropertyRequest request, string userId, CancellationToken ct);
    Task<ContractsResult<bool>> UpdateAsync(int id, UpdatePropertyRequest request, string userId, bool isAdmin, CancellationToken ct);
    Task<ContractsResult<bool>> DeleteAsync(int id, string userId, bool isAdmin, CancellationToken ct);
    Task<IReadOnlyList<PropertyListItem>> GetMineAsync(string userId, CancellationToken ct);
    Task<IReadOnlyList<PropertyListItem>> GetPendingAsync(CancellationToken ct);
    Task<ContractsResult<bool>> ApproveAsync(int id, ApprovePropertyRequest request, CancellationToken ct);
}
public interface IBookingService
{
    Task<ContractsResult<BookingDto>> CreateAsync(int propertyId, string userId, CancellationToken ct);
    Task<IReadOnlyList<BookingDto>> GetMyAsync(string userId, CancellationToken ct);
    Task<IReadOnlyList<BookingDto>> GetVendorAsync(string vendorId, CancellationToken ct);
    Task<ContractsResult<bool>> ChangeStatusAsync(int bookingId, BookingStatus status, string userId, bool isVendor, CancellationToken ct);
}
public interface IFavoriteService
{
    Task<IReadOnlyList<FavoriteDto>> GetMineAsync(string userId, CancellationToken ct);
    Task<ContractsResult<bool>> AddAsync(int propertyId, string userId, CancellationToken ct);
    Task<ContractsResult<bool>> RemoveAsync(int propertyId, string userId, CancellationToken ct);
}
public interface IReviewService
{
    Task<IReadOnlyList<ReviewDto>> GetForPropertyAsync(int propertyId, CancellationToken ct);
    Task<ContractsResult<ReviewDto>> CreateAsync(CreateReviewRequest request, string userId, CancellationToken ct);
    Task<ContractsResult<bool>> UpdateAsync(int id, UpdateReviewRequest request, string userId, CancellationToken ct);
    Task<ContractsResult<bool>> DeleteAsync(int id, string userId, bool isAdmin, CancellationToken ct);
    Task<IReadOnlyList<ReviewDto>> GetMineAsync(string userId, CancellationToken ct);
}
public interface IAdminService
{
    Task<AdminDashboardDto> DashboardAsync(CancellationToken ct);
    Task<IReadOnlyList<UserSummary>> GetUsersAsync(CancellationToken ct);
    Task<ContractsResult<bool>> DeleteUserAsync(string userId, CancellationToken ct);
    Task<IReadOnlyList<PropertyListItem>> GetPropertiesAsync(CancellationToken ct);
}
public record UserSummary(string Id, string Name, string Email, string? PhoneNumber, UserRole Role, DateTime CreatedAt);
public interface ITokenService { string CreateToken(User user); }
public interface IImageStorage { Task<IReadOnlyList<string>> SaveAsync(IEnumerable<IFormFile> files, CancellationToken ct); Task DeleteAsync(IEnumerable<string> urls, CancellationToken ct); }
public record ContractsResult<T>(bool Success, T? Data, string? Error, int StatusCode)
{
    public static ContractsResult<T> Ok(T data) => new(true, data, null, 200);
    public static ContractsResult<T> Fail(string error, int statusCode) => new(false, default, error, statusCode);
}
