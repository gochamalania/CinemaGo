namespace CinemaGo.Application.DTOs.Users;

public class UserProfileResponseDto
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; }

    public DateTime CreatedAt { get; set; }
}