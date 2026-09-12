using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Api.Extensions;
using RealEstate.Application.Features.ViewingAppointments;
using RealEstate.Application.Interfaces;

namespace RealEstate.Api.Controllers;

[ApiController]
[Route("api/viewings")]
[Authorize]
public sealed class ViewingAppointmentsController(IViewingAppointmentService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateViewingAppointmentRequest request, CancellationToken ct) =>
        this.ToActionResult(await service.CreateAsync(request, UserId(), ct));

    [HttpGet("mine")]
    public async Task<IActionResult> Mine(CancellationToken ct) => Ok(await service.GetMineAsync(UserId(), ct));

    [HttpPatch("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct) =>
        this.ToActionResult(await service.ChangeStatusAsync(id, "Cancelled", UserId(), false, ct));

    [Authorize(Roles = "Vendor")]
    [HttpPatch("{id:int}/confirm")]
    public async Task<IActionResult> Confirm(int id, CancellationToken ct) =>
        this.ToActionResult(await service.ChangeStatusAsync(id, "Confirmed", UserId(), true, ct));

    [Authorize(Roles = "Vendor")]
    [HttpPatch("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, CancellationToken ct) =>
        this.ToActionResult(await service.ChangeStatusAsync(id, "Rejected", UserId(), true, ct));

    private string UserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
}
