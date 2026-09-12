using System.ComponentModel.DataAnnotations;

namespace RealEstate.Application.Features.ViewingAppointments;

public sealed record CreateViewingAppointmentRequest(
    [property:Range(1, int.MaxValue)] int PropertyId,
    DateTime StartAt,
    DateTime EndAt,
    [property:StringLength(1000)] string? Notes);

public sealed record ViewingAppointmentDto(int Id, int PropertyId, string PropertyTitle, string UserId, DateTime StartAt, DateTime EndAt, string Status, string? Notes, DateTime CreatedAt);
