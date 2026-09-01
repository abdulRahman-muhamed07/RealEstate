using Microsoft.EntityFrameworkCore;
using RealEstate.Application.Contracts;
using RealEstate.Domain.Entities;
using RealEstate.Infrastructure.Persistence;

namespace RealEstate.Infrastructure.Services;

public sealed class BookingService(AppDbContext db) : IBookingService
{
    public async Task<ContractsResult<BookingDto>> CreateAsync(int propertyId, string userId, CancellationToken ct)
    {
        var property = await db.Properties.FirstOrDefaultAsync(x => x.Id == propertyId && x.IsApproved, ct);
        if (property is null) return ContractsResult<BookingDto>.Fail("Property not found.", 404);
        if (await db.Bookings.AnyAsync(x => x.PropertyId == propertyId && x.UserId == userId && x.Status != BookingStatus.Cancelled, ct)) return ContractsResult<BookingDto>.Fail("You already have a booking for this property.", 409);
        var b = new Booking { PropertyId = propertyId, UserId = userId, BookingDate = DateTime.UtcNow }; db.Bookings.Add(b); await db.SaveChangesAsync(ct); return ContractsResult<BookingDto>.Ok(new BookingDto(b.Id,b.PropertyId,property.Title,b.UserId,"",b.BookingDate,b.Status,b.CreatedAt));
    }
    public async Task<IReadOnlyList<BookingDto>> GetMyAsync(string userId, CancellationToken ct) => await db.Bookings.AsNoTracking().Where(x => x.UserId == userId).Include(x => x.Property).Include(x => x.User).OrderByDescending(x => x.CreatedAt).Select(x => new BookingDto(x.Id,x.PropertyId,x.Property.Title,x.UserId,(x.User.FirstName+" "+x.User.LastName).Trim(),x.BookingDate,x.Status,x.CreatedAt)).ToListAsync(ct);
    public async Task<IReadOnlyList<BookingDto>> GetVendorAsync(string vendorId, CancellationToken ct) => await db.Bookings.AsNoTracking().Where(x => x.Property.OwnerId == vendorId).Include(x => x.Property).Include(x => x.User).OrderByDescending(x => x.CreatedAt).Select(x => new BookingDto(x.Id,x.PropertyId,x.Property.Title,x.UserId,(x.User.FirstName+" "+x.User.LastName).Trim(),x.BookingDate,x.Status,x.CreatedAt)).ToListAsync(ct);
    public async Task<ContractsResult<bool>> ChangeStatusAsync(int id, BookingStatus status, string userId, bool isVendor, CancellationToken ct)
    {
        var b = await db.Bookings.Include(x=>x.Property).FirstOrDefaultAsync(x=>x.Id==id,ct); if (b is null) return ContractsResult<bool>.Fail("Booking not found.",404);
        if ((status == BookingStatus.Cancelled && b.UserId != userId) || (status is BookingStatus.Confirmed or BookingStatus.Rejected) && (!isVendor || b.Property.OwnerId != userId)) return ContractsResult<bool>.Fail("Forbidden.",403);
        b.Status=status; if(status==BookingStatus.Confirmed) b.Property.Status=PropertyStatus.Booked; if(status==BookingStatus.Cancelled && b.Property.Status==PropertyStatus.Booked) b.Property.Status=PropertyStatus.Available; await db.SaveChangesAsync(ct); return ContractsResult<bool>.Ok(true);
    }
}

public sealed class FavoriteService(AppDbContext db) : IFavoriteService
{
    public async Task<IReadOnlyList<FavoriteDto>> GetMineAsync(string userId,CancellationToken ct)=>await db.Favorites.AsNoTracking().Where(x=>x.UserId==userId).Include(x=>x.Property).ThenInclude(x=>x.Images).Select(x=>new FavoriteDto(x.PropertyId,x.Property.Title,x.Property.Price,x.Property.Images.Select(i=>(string?)i.Url).FirstOrDefault(),x.Property.Location)).ToListAsync(ct);
    public async Task<ContractsResult<bool>> AddAsync(int propertyId,string userId,CancellationToken ct){if(!await db.Properties.AnyAsync(x=>x.Id==propertyId&&x.IsApproved,ct))return ContractsResult<bool>.Fail("Property not found.",404);if(!await db.Favorites.AnyAsync(x=>x.PropertyId==propertyId&&x.UserId==userId,ct)){db.Favorites.Add(new Favorite{PropertyId=propertyId,UserId=userId});await db.SaveChangesAsync(ct);}return ContractsResult<bool>.Ok(true);}
    public async Task<ContractsResult<bool>> RemoveAsync(int propertyId,string userId,CancellationToken ct){var f=await db.Favorites.FirstOrDefaultAsync(x=>x.PropertyId==propertyId&&x.UserId==userId,ct);if(f is null)return ContractsResult<bool>.Fail("Favorite not found.",404);db.Favorites.Remove(f);await db.SaveChangesAsync(ct);return ContractsResult<bool>.Ok(true);}
}

public sealed class ReviewService(AppDbContext db) : IReviewService
{
    public async Task<IReadOnlyList<ReviewDto>> GetForPropertyAsync(int propertyId,CancellationToken ct)=>await db.Reviews.AsNoTracking().Where(x=>x.PropertyId==propertyId).Include(x=>x.User).OrderByDescending(x=>x.CreatedAt).Select(x=>new ReviewDto(x.Id,x.UserId,(x.User.FirstName+" "+x.User.LastName).Trim(),x.Rating,x.Comment,x.CreatedAt)).ToListAsync(ct);
    public async Task<ContractsResult<ReviewDto>> CreateAsync(CreateReviewRequest r,string userId,CancellationToken ct){if(r.Rating<1||r.Rating>5)return ContractsResult<ReviewDto>.Fail("Rating must be between 1 and 5.",400);if(!await db.Properties.AnyAsync(x=>x.Id==r.PropertyId&&x.IsApproved,ct))return ContractsResult<ReviewDto>.Fail("Property not found.",404);if(await db.Reviews.AnyAsync(x=>x.PropertyId==r.PropertyId&&x.UserId==userId,ct))return ContractsResult<ReviewDto>.Fail("You already reviewed this property.",409);var x=new Review{PropertyId=r.PropertyId,UserId=userId,Rating=r.Rating,Comment=r.Comment.Trim()};db.Reviews.Add(x);await db.SaveChangesAsync(ct);return ContractsResult<ReviewDto>.Ok(new ReviewDto(x.Id,x.UserId,"",x.Rating,x.Comment,x.CreatedAt));}
    public async Task<ContractsResult<bool>> UpdateAsync(int id,UpdateReviewRequest r,string userId,CancellationToken ct){var x=await db.Reviews.FindAsync([id],ct);if(x is null)return ContractsResult<bool>.Fail("Review not found.",404);if(x.UserId!=userId)return ContractsResult<bool>.Fail("Forbidden.",403);if(r.Rating<1||r.Rating>5)return ContractsResult<bool>.Fail("Rating must be between 1 and 5.",400);x.Rating=r.Rating;x.Comment=r.Comment.Trim();await db.SaveChangesAsync(ct);return ContractsResult<bool>.Ok(true);}
    public async Task<ContractsResult<bool>> DeleteAsync(int id,string userId,bool isAdmin,CancellationToken ct){var x=await db.Reviews.FindAsync([id],ct);if(x is null)return ContractsResult<bool>.Fail("Review not found.",404);if(!isAdmin&&x.UserId!=userId)return ContractsResult<bool>.Fail("Forbidden.",403);db.Reviews.Remove(x);await db.SaveChangesAsync(ct);return ContractsResult<bool>.Ok(true);}
    public async Task<IReadOnlyList<ReviewDto>> GetMineAsync(string userId,CancellationToken ct)=>await db.Reviews.AsNoTracking().Where(x=>x.UserId==userId).Include(x=>x.User).Select(x=>new ReviewDto(x.Id,x.UserId,(x.User.FirstName+" "+x.User.LastName).Trim(),x.Rating,x.Comment,x.CreatedAt)).ToListAsync(ct);
}

public sealed class AdminService(AppDbContext db) : IAdminService
{
    public async Task<AdminDashboardDto> DashboardAsync(CancellationToken ct)=>new(await db.Users.CountAsync(ct),await db.Properties.CountAsync(ct),await db.Properties.CountAsync(x=>!x.IsApproved,ct),await db.Bookings.CountAsync(ct),await db.Reviews.CountAsync(ct));
    public async Task<IReadOnlyList<UserSummary>> GetUsersAsync(CancellationToken ct)=>await db.Users.AsNoTracking().OrderByDescending(x=>x.CreatedAt).Select(x=>new UserSummary(x.Id,(x.FirstName+" "+x.LastName).Trim(),x.Email,x.PhoneNumber,x.Role,x.CreatedAt)).ToListAsync(ct);
    public async Task<ContractsResult<bool>> DeleteUserAsync(string userId,CancellationToken ct){var u=await db.Users.FindAsync([userId],ct);if(u is null)return ContractsResult<bool>.Fail("User not found.",404);if(u.Role==UserRole.Admin)return ContractsResult<bool>.Fail("Admin cannot be deleted.",400);db.Users.Remove(u);await db.SaveChangesAsync(ct);return ContractsResult<bool>.Ok(true);}
    public async Task<IReadOnlyList<PropertyListItem>> GetPropertiesAsync(CancellationToken ct)=>await db.Properties.AsNoTracking().Include(x=>x.Category).Include(x=>x.City).Include(x=>x.Owner).Include(x=>x.Images).OrderByDescending(x=>x.CreatedAt).Select(p=>new PropertyListItem(p.Id,p.Title,p.Price,p.Area,p.Bedrooms,p.Bathrooms,p.Type,p.ListingType,p.Status,p.Location,p.CreatedAt,p.CategoryId,p.Category.Name,p.CityId,p.City==null?null:p.City.Name,p.OwnerId,(p.Owner.FirstName+" "+p.Owner.LastName).Trim(),p.Images.Select(i=>i.Url).ToList())).ToListAsync(ct);
}
