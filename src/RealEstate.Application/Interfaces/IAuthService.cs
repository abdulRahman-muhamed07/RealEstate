using RealEstate.Application.Common;
using RealEstate.Application.Features.Auth;

namespace RealEstate.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct);
}
