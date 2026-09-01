using RealEstate.Application.Common;
using RealEstate.Application.Features.Bookings;
using RealEstate.Domain.Entities;

namespace RealEstate.Application.Interfaces;

public interface IBookingService
{
    Task<Result<BookingDto>> CreateAsync(int propertyId, string userId, CancellationToken ct);
    Task<IReadOnlyList<BookingDto>> GetMyAsync(string userId, CancellationToken ct);
    Task<IReadOnlyList<BookingDto>> GetVendorAsync(string vendorId, CancellationToken ct);
    Task<Result<bool>> ChangeStatusAsync(int id, BookingStatus status, string userId, bool isVendor, CancellationToken ct);
}
