using Microsoft.EntityFrameworkCore;
using RealEstate.Application.Common;
using RealEstate.Application.Features.ViewingAppointments;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Entities;
using RealEstate.Infrastructure.Persistence;

namespace RealEstate.Infrastructure.Services;

public sealed class ViewingAppointmentService(AppDbContext db) : IViewingAppointmentService
{
    public async Task<Result<ViewingAppointmentDto>> CreateAsync(CreateViewingAppointmentRequest request, string userId, CancellationToken ct)
    {
        if (request.EndAt <= request.StartAt)
            return Result<ViewingAppointmentDto>.Fail(ErrorCode.Validation, "End time must be after start time.");
        if (request.StartAt <= DateTime.UtcNow)
            return Result<ViewingAppointmentDto>.Fail(ErrorCode.Validation, "Viewing time must be in the future.");

        var property = await db.Properties.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.PropertyId && x.IsApproved, ct);
        if (property is null)
            return Result<ViewingAppointmentDto>.Fail(ErrorCode.NotFound, "Property not found.");
        if (property.OwnerId == userId)
            return Result<ViewingAppointmentDto>.Fail(ErrorCode.InvalidOperation, "You cannot schedule a viewing for your own property.");

        var overlap = await db.ViewingAppointments.AnyAsync(x => x.PropertyId == request.PropertyId &&
            x.Status is ViewingAppointmentStatus.Pending or ViewingAppointmentStatus.Confirmed &&
            request.StartAt < x.EndAt && request.EndAt > x.StartAt, ct);
        if (overlap)
            return Result<ViewingAppointmentDto>.Fail(ErrorCode.Conflict, "The selected time is already booked.");

        var appointment = new ViewingAppointment
        {
            PropertyId = request.PropertyId,
            UserId = userId,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            Notes = request.Notes?.Trim()
        };
        db.ViewingAppointments.Add(appointment);
        await db.SaveChangesAsync(ct);

        return Result<ViewingAppointmentDto>.Ok(new ViewingAppointmentDto(
            appointment.Id, property.Id, property.Title, userId, appointment.StartAt,
            appointment.EndAt, appointment.Status.ToString(), appointment.Notes, appointment.CreatedAt));
    }

    public async Task<IReadOnlyList<ViewingAppointmentDto>> GetMineAsync(string userId, CancellationToken ct) =>
        await db.ViewingAppointments.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.StartAt)
            .Select(x => new ViewingAppointmentDto(x.Id, x.PropertyId, x.Property.Title, x.UserId, x.StartAt, x.EndAt, x.Status.ToString(), x.Notes, x.CreatedAt))
            .ToListAsync(ct);

    public async Task<Result<bool>> ChangeStatusAsync(int id, string status, string userId, bool isVendor, CancellationToken ct)
    {
        var appointment = await db.ViewingAppointments.Include(x => x.Property).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (appointment is null)
            return Result<bool>.Fail(ErrorCode.NotFound, "Viewing appointment not found.");
        if (!Enum.TryParse<ViewingAppointmentStatus>(status, true, out var parsed))
            return Result<bool>.Fail(ErrorCode.Validation, "Invalid appointment status.");

        var allowed = parsed == ViewingAppointmentStatus.Cancelled
            ? appointment.UserId == userId && appointment.Status == ViewingAppointmentStatus.Pending
            : isVendor && appointment.Property.OwnerId == userId && appointment.Status == ViewingAppointmentStatus.Pending;
        if (!allowed)
            return Result<bool>.Fail(ErrorCode.Forbidden, "You are not allowed to change this appointment.");

        appointment.Status = parsed;
        await db.SaveChangesAsync(ct);
        return Result<bool>.Ok(true);
    }
}
