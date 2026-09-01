using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealEstate.Application.Contracts;
using RealEstate.Domain.Entities;
using RealEstate.Infrastructure.Persistence;

namespace RealEstate.Infrastructure.Services;

public sealed class AuthService(AppDbContext db, ITokenService tokens) : IAuthService
{
    private readonly PasswordHasher<User> _hasher = new();

    public async Task<ContractsResult<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(x => x.Email == email, ct)) return ContractsResult<AuthResponse>.Fail("Email already exists.", 409);
        if (request.Role == UserRole.Admin) return ContractsResult<AuthResponse>.Fail("Admin registration is not allowed.", 403);
        var user = new User { FirstName = request.FirstName.Trim(), LastName = request.LastName.Trim(), Email = email, PhoneNumber = request.PhoneNumber, Role = request.Role };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);
        db.Users.Add(user); await db.SaveChangesAsync(ct);
        return ContractsResult<AuthResponse>.Ok(Map(user));
    }

    public async Task<ContractsResult<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email, ct);
        if (user is null) return ContractsResult<AuthResponse>.Fail("Invalid email or password.", 401);
        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        return result == PasswordVerificationResult.Failed
            ? ContractsResult<AuthResponse>.Fail("Invalid email or password.", 401)
            : ContractsResult<AuthResponse>.Ok(Map(user));
    }

    private AuthResponse Map(User u) => new(tokens.CreateToken(u), u.Id, u.FirstName, u.LastName, u.Email, u.Role);
}
