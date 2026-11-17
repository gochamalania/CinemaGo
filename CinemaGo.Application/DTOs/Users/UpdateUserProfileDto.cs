using System.ComponentModel.DataAnnotations;

namespace CinemaGo.Application.DTOs.Users;

public class UpdateUserProfileDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public DateTime? DateOfBirth { get; set; }
}