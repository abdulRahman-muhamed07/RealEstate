using Microsoft.EntityFrameworkCore;
using RealEstate.Application.Common;
using RealEstate.Application.Features.Bookings;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Domain.Enums;
using RealEstate.Infrastructure.Persistence;

namespace RealEstate.Infrastructure.Services;

public sealed class BookingService(AppDbContext db) : IBookingService
{
    public async Task<Result<BookingDto>> CreateAsync(int propertyId, string userId, CancellationToken ct)
    {
        var property = await db.Properties.FirstOrDefaultAsync(x => x.Id == propertyId && x.IsApproved, ct);
        if (property is null) return Result<BookingDto>.Fail(ErrorCode.NotFound, "Property not found.");
        if (property.Status is PropertyStatus.Sold or PropertyStatus.Booked) return Result<BookingDto>.Fail(ErrorCode.Conflict, "Property is not available for booking.");
        if (property.OwnerId == userId) return Result<BookingDto>.Fail(ErrorCode.InvalidOperation, "You cannot book your own property.");
        if (await db.Bookings.AnyAsync(x => x.PropertyId == propertyId && x.UserId == userId && x.Status != BookingStatus.Cancelled && x.Status != BookingStatus.Rejected, ct))
            return Result<BookingDto>.Fail(ErrorCode.Conflict, "You already have an active booking for this property.");

        var booking = new Booking { PropertyId = propertyId, UserId = userId };
        db.Bookings.Add(booking);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return Result<BookingDto>.Fail(ErrorCode.Conflict, "A booking already exists for this property.");
        }

        return Result<BookingDto>.Ok(new BookingDto(booking.Id, booking.PropertyId, property.Title, booking.UserId, string.Empty, booking.BookingDate, booking.Status, booking.CreatedAt));
    }

    public async Task<IReadOnlyList<BookingDto>> GetMyAsync(string userId, CancellationToken ct) =>
        await db.Bookings.AsNoTracking().Where(x => x.UserId == userId).Include(x => x.Property).Include(x => x.User)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new BookingDto(x.Id, x.PropertyId, x.Property.Title, x.UserId, (x.User.FirstName + " " + x.User.LastName).Trim(), x.BookingDate, x.Status, x.CreatedAt))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<BookingDto>> GetVendorAsync(string vendorId, CancellationToken ct) =>
        await db.Bookings.AsNoTracking().Where(x => x.Property.OwnerId == vendorId).Include(x => x.Property).Include(x => x.User)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new BookingDto(x.Id, x.PropertyId, x.Property.Title, x.UserId, (x.User.FirstName + " " + x.User.LastName).Trim(), x.BookingDate, x.Status, x.CreatedAt))
            .ToListAsync(ct);

    public async Task<Result<bool>> ChangeStatusAsync(int id, BookingStatus status, string userId, bool isVendor, CancellationToken ct)
    {
        var booking = await db.Bookings.Include(x => x.Property).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (booking is null) return Result<bool>.Fail(ErrorCode.NotFound, "Booking not found.");

        var allowed = status switch
        {
            BookingStatus.Cancelled => booking.UserId == userId && booking.Status == BookingStatus.Pending,
            BookingStatus.Confirmed or BookingStatus.Rejected => isVendor && booking.Property.OwnerId == userId && booking.Status == BookingStatus.Pending,
            _ => false
        };
        if (!allowed) return Result<bool>.Fail(ErrorCode.Forbidden, "You are not allowed to change this booking.");

        if (status == BookingStatus.Confirmed)
        {
            var activeBookingExists = await db.Bookings.AnyAsync(x => x.PropertyId == booking.PropertyId && x.Id != booking.Id && x.Status == BookingStatus.Confirmed, ct);
            if (activeBookingExists) return Result<bool>.Fail(ErrorCode.Conflict, "Another booking is already confirmed for this property.");
            booking.Property.Status = PropertyStatus.Booked;
        }
        if (status == BookingStatus.Cancelled && booking.Property.Status == PropertyStatus.Booked) booking.Property.Status = PropertyStatus.Available;
        booking.Status = status;
        await db.SaveChangesAsync(ct);
        return Result<bool>.Ok(true);
    }
}
