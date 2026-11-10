using CinemaGo.Domain.Entities;

namespace CinemaGo.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}