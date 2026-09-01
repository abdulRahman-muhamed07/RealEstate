using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Api.Extensions;
using RealEstate.Application.Interfaces;

namespace RealEstate.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public sealed class AdminController(IAdminService adminService, IPropertyCommandService propertyCommandService) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct) => Ok(await adminService.DashboardAsync(ct));

    [HttpGet("users")]
    public async Task<IActionResult> Users(CancellationToken ct) => Ok(await adminService.GetUsersAsync(ct));

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(string id, CancellationToken ct) =>
        this.ToActionResult(await adminService.DeleteUserAsync(id, ct));

    [HttpGet("properties")]
    public async Task<IActionResult> Properties(CancellationToken ct) => Ok(await adminService.GetPropertiesAsync(ct));

    [HttpDelete("properties/{id:int}")]
    public async Task<IActionResult> DeleteProperty(int id, CancellationToken ct) =>
        this.ToActionResult(await propertyCommandService.DeleteByAdminAsync(id, ct));
}