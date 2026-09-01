using RealEstate.Domain.Entities;

namespace RealEstate.Application.Interfaces;

public interface ITokenService
{
    string CreateToken(User user);
}
