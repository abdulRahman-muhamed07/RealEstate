using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Api.Extensions;
using RealEstate.Application.Interfaces;

namespace RealEstate.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public sealed class AdminController(IAdminService service, IPropertyService propertyService) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct) => Ok(await service.DashboardAsync(ct));

    [HttpGet("users")]
    public async Task<IActionResult> Users(CancellationToken ct) => Ok(await service.GetUsersAsync(ct));

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(string id, CancellationToken ct) => this.ToActionResult(await service.DeleteUserAsync(id, ct));

    [HttpGet("properties")]
    public async Task<IActionResult> Properties(CancellationToken ct) => Ok(await service.GetPropertiesAsync(ct));

    [HttpDelete("properties/{id:int}")]
    public async Task<IActionResult> DeleteProperty(int id, CancellationToken ct) =>
        this.ToActionResult(await propertyService.DeleteAsync(id, UserId(), true, ct));

    private string UserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException();
}
