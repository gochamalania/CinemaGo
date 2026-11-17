using CinemaGo.Application.DTOs.Users;

namespace CinemaGo.Application.Interfaces;

public interface IUserService
{
    Task<UserProfileResponseDto> GetMyProfileAsync(int userId);

    Task<UserProfileResponseDto> UpdateMyProfileAsync(
        int userId,
        UpdateUserProfileDto dto);
}