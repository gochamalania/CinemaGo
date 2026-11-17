using System.ComponentModel.DataAnnotations;

namespace CinemaGo.Application.DTOs.Movies;

public class CreateMovieDto : IValidatableObject
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Range(1, 1000)]
    public int DurationMinutes { get; set; }

    [Required]
    public DateTime ReleaseDate { get; set; }

    [Range(0, 18)]
    public int AgeRestriction { get; set; } = 0;

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            yield return new ValidationResult(
                "Movie title cannot be empty.",
                new[] { nameof(Title) });
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            yield return new ValidationResult(
                "Movie description cannot be empty.",
                new[] { nameof(Description) });
        }

        if (ReleaseDate == default)
        {
            yield return new ValidationResult(
                "A valid release date is required.",
                new[] { nameof(ReleaseDate) });
        }

        if (AgeRestriction != 0 &&
            AgeRestriction != 12 &&
            AgeRestriction != 16 &&
            AgeRestriction != 18)
        {
            yield return new ValidationResult(
                "Age restriction must be 0, 12, 16 or 18.",
                new[] { nameof(AgeRestriction) });
        }
    }
}