namespace CinemaGo.Application.DTOs.Reservations;

public class ReservationResponseDto
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int SessionId { get; set; }

    public int SeatId { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }
}