using System.ComponentModel.DataAnnotations;

namespace CinemaGo.Application.DTOs.Halls;

public class UpdateHallDto : IValidatableObject
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 1000)]
    public int Capacity { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult(
                "Hall name cannot be empty.",
                new[] { nameof(Name) });
        }
    }
}