using System.ComponentModel.DataAnnotations;

namespace CinemaGo.Application.DTOs.Tickets;

public class CreateTicketDto : IValidatableObject
{
    [Range(1, int.MaxValue,
        ErrorMessage = "Reservation ID must be greater than zero.")]
    public int ReservationId { get; set; }

    [Required]
    public string TicketType { get; set; } = "Adult";

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        var allowedTypes = new[]
        {
            "Adult",
            "Child",
            "Student"
        };

        if (!allowedTypes.Contains(TicketType))
        {
            yield return new ValidationResult(
                "Ticket type must be Adult, Child or Student.",
                new[] { nameof(TicketType) });
        }
    }
}