
using CinemaGo.Application.DTOs.Users;
using CinemaGo.Application.Interfaces;
using CinemaGo.Domain.Entities;

namespace CinemaGo.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserProfileResponseDto> GetMyProfileAsync(
        int userId)
    {
        if (userId <= 0)
            throw new ArgumentException("Invalid user.");

        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            throw new KeyNotFoundException("User not found.");

        return MapToDto(user);
    }

    public async Task<UserProfileResponseDto> UpdateMyProfileAsync(
        int userId,
        UpdateUserProfileDto dto)
    {
        if (userId <= 0)
            throw new ArgumentException("Invalid user.");

        ArgumentNullException.ThrowIfNull(dto);

        var firstName = dto.FirstName?.Trim();
        var lastName = dto.LastName?.Trim();

        if (string.IsNullOrWhiteSpace(firstName) ||
            firstName.Length < 2 ||
            firstName.Length > 100)
        {
            throw new ArgumentException(
                "First name must contain between 2 and 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(lastName) ||
            lastName.Length < 2 ||
            lastName.Length > 100)
        {
            throw new ArgumentException(
                "Last name must contain between 2 and 100 characters.");
        }

        if (!dto.DateOfBirth.HasValue)
            throw new ArgumentException(
                "Date of birth is required.");

        var today = DateTime.UtcNow.Date;
        var dateOfBirth = dto.DateOfBirth.Value.Date;

        if (dateOfBirth > today)
            throw new ArgumentException(
                "Date of birth cannot be in the future.");

        if (dateOfBirth < today.AddYears(-120))
            throw new ArgumentException(
                "Date of birth cannot be more than 120 years ago.");

        var updatedUser = await _userRepository.UpdateProfileAsync(
            userId,
            firstName,
            lastName,
            dateOfBirth);

        if (updatedUser == null)
            throw new KeyNotFoundException("User not found.");

        return MapToDto(updatedUser);
    }

    private static UserProfileResponseDto MapToDto(User user)
    {
        return new UserProfileResponseDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role,
            DateOfBirth = user.DateOfBirth,
            CreatedAt = user.CreatedAt
        };
    }
}
