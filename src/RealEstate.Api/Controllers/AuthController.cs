using Microsoft.AspNetCore.Mvc;
using RealEstate.Api.Extensions;
using RealEstate.Application.Features.Auth;
using RealEstate.Application.Interfaces;

namespace RealEstate.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService service) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct) =>
        this.ToActionResult<AuthResponse>(await service.RegisterAsync(request, ct));

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct) =>
        this.ToActionResult<AuthResponse>(await service.LoginAsync(request, ct));

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken ct) =>
        this.ToActionResult<AuthResponse>(await service.RefreshAsync(request.RefreshToken, ct));
}
