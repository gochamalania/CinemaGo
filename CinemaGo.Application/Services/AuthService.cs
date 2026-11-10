using CinemaGo.Application.DTOs.Auth;
using CinemaGo.Application.Interfaces;
using CinemaGo.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CinemaGo.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher,
        IJwtTokenService jwtTokenService
        )
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }
    
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        string email = request.Email.Trim();

        var existingUser = await _userRepository.GetByEmailAsync(email);

        if (existingUser != null)
        {
            throw new InvalidOperationException(
                "User with this email already exists.");
        }

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            Role = "Customer",
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            request.Password
        );

        await _userRepository.AddAsync(user);

        return new AuthResponseDto
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            Token = string.Empty,
            ExpiresAt = default
        };
    }


    public Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        throw new NotImplementedException();
    }
}