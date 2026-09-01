using Microsoft.EntityFrameworkCore;
using RealEstate.Application.Common;
using RealEstate.Application.Features.Reviews;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Infrastructure.Persistence;

namespace RealEstate.Infrastructure.Services;

public sealed class ReviewService(AppDbContext db) : IReviewService
{
    public async Task<IReadOnlyList<ReviewDto>> GetForPropertyAsync(int propertyId, CancellationToken ct) =>
        await db.Reviews.AsNoTracking().Where(x => x.PropertyId == propertyId).Include(x => x.User)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ReviewDto(x.Id, x.UserId, (x.User.FirstName + " " + x.User.LastName).Trim(), x.Rating, x.Comment, x.CreatedAt))
            .ToListAsync(ct);

    public async Task<Result<ReviewDto>> CreateAsync(CreateReviewRequest request, string userId, CancellationToken ct)
    {
        if (!await db.Properties.AnyAsync(x => x.Id == request.PropertyId && x.IsApproved, ct))
            return Result<ReviewDto>.Fail(ErrorCode.NotFound, "Property not found.");
        if (await db.Reviews.AnyAsync(x => x.PropertyId == request.PropertyId && x.UserId == userId, ct))
            return Result<ReviewDto>.Fail(ErrorCode.Conflict, "You already reviewed this property.");

        var review = new Review { PropertyId = request.PropertyId, UserId = userId, Rating = request.Rating, Comment = request.Comment.Trim() };
        db.Reviews.Add(review);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException) { return Result<ReviewDto>.Fail(ErrorCode.Conflict, "You already reviewed this property."); }
        return Result<ReviewDto>.Ok(new ReviewDto(review.Id, review.UserId, string.Empty, review.Rating, review.Comment, review.CreatedAt));
    }

    public async Task<Result<bool>> UpdateAsync(int id, UpdateReviewRequest request, string userId, CancellationToken ct)
    {
        var review = await db.Reviews.FindAsync([id], ct);
        if (review is null) return Result<bool>.Fail(ErrorCode.NotFound, "Review not found.");
        if (review.UserId != userId) return Result<bool>.Fail(ErrorCode.Forbidden, "You cannot update this review.");
        review.Rating = request.Rating;
        review.Comment = request.Comment.Trim();
        await db.SaveChangesAsync(ct);
        return Result<bool>.Ok(true);
    }

    public async Task<Result<bool>> DeleteAsync(int id, string userId, bool isAdmin, CancellationToken ct)
    {
        var review = await db.Reviews.FindAsync([id], ct);
        if (review is null) return Result<bool>.Fail(ErrorCode.NotFound, "Review not found.");
        if (!isAdmin && review.UserId != userId) return Result<bool>.Fail(ErrorCode.Forbidden, "You cannot delete this review.");
        db.Reviews.Remove(review);
        await db.SaveChangesAsync(ct);
        return Result<bool>.Ok(true);
    }

    public async Task<IReadOnlyList<ReviewDto>> GetMineAsync(string userId, CancellationToken ct) =>
        await db.Reviews.AsNoTracking().Where(x => x.UserId == userId).Include(x => x.User)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new ReviewDto(x.Id, x.UserId, (x.User.FirstName + " " + x.User.LastName).Trim(), x.Rating, x.Comment, x.CreatedAt))
            .ToListAsync(ct);
}
