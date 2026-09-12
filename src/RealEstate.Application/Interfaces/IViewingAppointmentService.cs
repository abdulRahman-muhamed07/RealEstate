using RealEstate.Application.Common;
using RealEstate.Application.Features.ViewingAppointments;

namespace RealEstate.Application.Interfaces;

public interface IViewingAppointmentService
{
    Task<Result<ViewingAppointmentDto>> CreateAsync(CreateViewingAppointmentRequest request, string userId, CancellationToken ct);
    Task<IReadOnlyList<ViewingAppointmentDto>> GetMineAsync(string userId, CancellationToken ct);
    Task<Result<bool>> ChangeStatusAsync(int id, string status, string userId, bool isVendor, CancellationToken ct);
}
