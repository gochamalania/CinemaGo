using System.ComponentModel.DataAnnotations;

namespace CinemaGo.Application.DTOs.Reservations;

public class CreateReservationDto
{
    [Range(1, int.MaxValue,
        ErrorMessage = "Session ID must be greater than zero.")]
    public int SessionId { get; set; }

    [Range(1, int.MaxValue,
        ErrorMessage = "Seat ID must be greater than zero.")]
    public int SeatId { get; set; }
}