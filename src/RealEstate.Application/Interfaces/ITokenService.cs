using RealEstate.Domain.Entities;

namespace RealEstate.Application.Interfaces;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateAccessToken(User user);
    string CreateRefreshToken();
}
