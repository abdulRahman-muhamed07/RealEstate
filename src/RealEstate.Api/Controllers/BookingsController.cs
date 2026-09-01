using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Api.Extensions;
using RealEstate.Application.Features.Bookings;
using RealEstate.Application.Interfaces;
using RealEstate.Domain.Enums;

namespace RealEstate.Api.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public sealed class BookingsController(IBookingService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateBookingRequest request, CancellationToken ct) =>
        this.ToActionResult(await service.CreateAsync(request.PropertyId, UserId(), ct));

    [HttpGet("my-bookings")]
    public async Task<IActionResult> Mine(CancellationToken ct) => Ok(await service.GetMyAsync(UserId(), ct));

    [Authorize(Roles = "Vendor")]
    [HttpGet("vendor/all")]
    public async Task<IActionResult> Vendor(CancellationToken ct) => Ok(await service.GetVendorAsync(UserId(), ct));

    [HttpPatch("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct) =>
        this.ToActionResult(await service.ChangeStatusAsync(id, BookingStatus.Cancelled, UserId(), false, ct));

    [Authorize(Roles = "Vendor")]
    [HttpPatch("{id:int}/confirm")]
    public async Task<IActionResult> Confirm(int id, CancellationToken ct) =>
        this.ToActionResult(await service.ChangeStatusAsync(id, BookingStatus.Confirmed, UserId(), true, ct));

    [Authorize(Roles = "Vendor")]
    [HttpPatch("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, CancellationToken ct) =>
        this.ToActionResult(await service.ChangeStatusAsync(id, BookingStatus.Rejected, UserId(), true, ct));

    private string UserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
}
