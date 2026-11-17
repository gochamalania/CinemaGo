using System.ComponentModel.DataAnnotations;

namespace CinemaGo.Application.DTOs.Sessions;

public class UpdateSessionDto : IValidatableObject
{
    [Range(1, int.MaxValue)]
    public int MovieId { get; set; }

    [Range(1, int.MaxValue)]
    public int HallId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Range(typeof(decimal), "0.01", "999999.99")]
    public decimal BasePrice { get; set; }

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (StartTime == default)
        {
            yield return new ValidationResult(
                "A valid session start time is required.",
                new[] { nameof(StartTime) });
        }
    }
}