using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Application.Contracts;
using RealEstate.Domain.Entities;
using System.Security.Claims;

namespace RealEstate.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService service) : ControllerBase
{
    [HttpPost("register")] public async Task<IActionResult> Register(RegisterRequest request,CancellationToken ct){var r=await service.RegisterAsync(request,ct);return r.Success?Ok(r.Data):StatusCode(r.StatusCode,new{message=r.Error});}
    [HttpPost("login")] public async Task<IActionResult> Login(LoginRequest request,CancellationToken ct){var r=await service.LoginAsync(request,ct);return r.Success?Ok(r.Data):StatusCode(r.StatusCode,new{message=r.Error});}
}

[ApiController]
[Route("api/properties")]
public sealed class PropertiesController(IPropertyService service) : ControllerBase
{
    [HttpGet] public Task<PagedResult<PropertyListItem>> Get([FromQuery]PropertyFilterRequest request,CancellationToken ct)=>service.SearchAsync(request,UserId(),ct);
    [HttpGet("search")] public Task<PagedResult<PropertyListItem>> Search([FromQuery]PropertyFilterRequest request,CancellationToken ct)=>service.SearchAsync(request,UserId(),ct);
    [HttpGet("{id:int}")] public async Task<IActionResult> GetById(int id,CancellationToken ct)=>OkOrNotFound(await service.GetByIdAsync(id,ct));
    [Authorize(Roles="Vendor,Admin")][HttpPost] public async Task<IActionResult> Create([FromForm]CreatePropertyRequest request,CancellationToken ct){var r=await service.CreateAsync(request,UserId()!,ct);return r.Success?Created($"/api/properties/{r.Data}",new{id=r.Data}):StatusCode(r.StatusCode,new{message=r.Error});}
    [Authorize(Roles="Vendor,Admin")][HttpPut("{id:int}")] public async Task<IActionResult> Update(int id,[FromForm]UpdatePropertyRequest request,CancellationToken ct){var r=await service.UpdateAsync(id,request,UserId()!,IsAdmin(),ct);return r.Success?Ok(new{message="Property updated."}):StatusCode(r.StatusCode,new{message=r.Error});}
    [Authorize(Roles="Vendor,Admin")][HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id,CancellationToken ct){var r=await service.DeleteAsync(id,UserId()!,IsAdmin(),ct);return r.Success?Ok(new{message="Property deleted."}):StatusCode(r.StatusCode,new{message=r.Error});}
    [Authorize][HttpGet("mine")] public Task<IReadOnlyList<PropertyListItem>> Mine(CancellationToken ct)=>service.GetMineAsync(UserId()!,ct);
    [Authorize(Roles="Admin")][HttpGet("admin/pending")] public Task<IReadOnlyList<PropertyListItem>> Pending(CancellationToken ct)=>service.GetPendingAsync(ct);
    [Authorize(Roles="Admin")][HttpPatch("admin/{id:int}/status")] public async Task<IActionResult> Approve(int id,ApprovePropertyRequest request,CancellationToken ct){var r=await service.ApproveAsync(id,request,ct);return r.Success?Ok(new{message=request.Approve?"Property approved.":"Property rejected."}):StatusCode(r.StatusCode,new{message=r.Error});}
    private string? UserId()=>User.FindFirstValue(ClaimTypes.NameIdentifier);
    private bool IsAdmin()=>User.IsInRole(nameof(UserRole.Admin));
    private static IActionResult OkOrNotFound(object? o)=>o is null?new NotFoundResult():new OkObjectResult(o);
}

[ApiController]
[Route("api/bookings")]
[Authorize]
public sealed class BookingsController(IBookingService service) : ControllerBase
{
    [HttpPost] public async Task<IActionResult> Create([FromBody]int propertyId,CancellationToken ct){var r=await service.CreateAsync(propertyId,UserId()!,ct);return r.Success?Ok(r.Data):StatusCode(r.StatusCode,new{message=r.Error});}
    [HttpGet("my-bookings")] public Task<IReadOnlyList<BookingDto>> Mine(CancellationToken ct)=>service.GetMyAsync(UserId()!,ct);
    [Authorize(Roles="Vendor")][HttpGet("vendor/all")] public Task<IReadOnlyList<BookingDto>> Vendor(CancellationToken ct)=>service.GetVendorAsync(UserId()!,ct);
    [HttpPatch("{id:int}/cancel")] public Task<IActionResult> Cancel(int id,CancellationToken ct)=>Change(id,BookingStatus.Cancelled,false,ct);
    [Authorize(Roles="Vendor")][HttpPatch("{id:int}/confirm")] public Task<IActionResult> Confirm(int id,CancellationToken ct)=>Change(id,BookingStatus.Confirmed,true,ct);
    [Authorize(Roles="Vendor")][HttpPatch("{id:int}/reject")] public Task<IActionResult> Reject(int id,CancellationToken ct)=>Change(id,BookingStatus.Rejected,true,ct);
    private async Task<IActionResult> Change(int id,BookingStatus status,bool vendor,CancellationToken ct){var r=await service.ChangeStatusAsync(id,status,UserId()!,vendor,ct);return r.Success?Ok(new{message="Booking status updated."}):StatusCode(r.StatusCode,new{message=r.Error});}
    private string? UserId()=>User.FindFirstValue(ClaimTypes.NameIdentifier);
}

[ApiController]
[Route("api/favorites")]
[Authorize]
public sealed class FavoritesController(IFavoriteService service):ControllerBase
{
    [HttpGet] public Task<IReadOnlyList<FavoriteDto>> Get(CancellationToken ct)=>service.GetMineAsync(UserId()!,ct);
    [HttpPost] public async Task<IActionResult> Add([FromBody]int propertyId,CancellationToken ct){var r=await service.AddAsync(propertyId,UserId()!,ct);return r.Success?Ok(new{message="Added to favorites."}):StatusCode(r.StatusCode,new{message=r.Error});}
    [HttpDelete("{propertyId:int}")] public async Task<IActionResult> Remove(int propertyId,CancellationToken ct){var r=await service.RemoveAsync(propertyId,UserId()!,ct);return r.Success?Ok(new{message="Removed from favorites."}):StatusCode(r.StatusCode,new{message=r.Error});}
    private string? UserId()=>User.FindFirstValue(ClaimTypes.NameIdentifier);
}

[ApiController]
[Route("api/reviews")]
public sealed class ReviewsController(IReviewService service):ControllerBase
{
    [AllowAnonymous][HttpGet("property/{propertyId:int}")] public Task<IReadOnlyList<ReviewDto>> GetForProperty(int propertyId,CancellationToken ct)=>service.GetForPropertyAsync(propertyId,ct);
    [Authorize][HttpPost] public async Task<IActionResult> Create(CreateReviewRequest request,CancellationToken ct){var r=await service.CreateAsync(request,UserId()!,ct);return r.Success?Ok(r.Data):StatusCode(r.StatusCode,new{message=r.Error});}
    [Authorize][HttpPut("{id:int}")] public async Task<IActionResult> Update(int id,UpdateReviewRequest request,CancellationToken ct){var r=await service.UpdateAsync(id,request,UserId()!,ct);return r.Success?Ok(new{message="Review updated."}):StatusCode(r.StatusCode,new{message=r.Error});}
    [Authorize][HttpDelete("{id:int}")] public async Task<IActionResult> Delete(int id,CancellationToken ct){var r=await service.DeleteAsync(id,UserId()!,User.IsInRole("Admin"),ct);return r.Success?Ok(new{message="Review deleted."}):StatusCode(r.StatusCode,new{message=r.Error});}
    [Authorize][HttpGet("my-reviews")] public Task<IReadOnlyList<ReviewDto>> Mine(CancellationToken ct)=>service.GetMineAsync(UserId()!,ct);
    private string? UserId()=>User.FindFirstValue(ClaimTypes.NameIdentifier);
}

[ApiController]
[Route("api/admin")]
[Authorize(Roles="Admin")]
public sealed class AdminController(IAdminService service):ControllerBase
{
    [HttpGet("dashboard")] public Task<AdminDashboardDto> Dashboard(CancellationToken ct)=>service.DashboardAsync(ct);
    [HttpGet("users")] public Task<IReadOnlyList<UserSummary>> Users(CancellationToken ct)=>service.GetUsersAsync(ct);
    [HttpDelete("users/{id}")] public async Task<IActionResult> DeleteUser(string id,CancellationToken ct){var r=await service.DeleteUserAsync(id,ct);return r.Success?Ok(new{message="User deleted."}):StatusCode(r.StatusCode,new{message=r.Error});}
    [HttpGet("properties")] public Task<IReadOnlyList<PropertyListItem>> Properties(CancellationToken ct)=>service.GetPropertiesAsync(ct);
}
