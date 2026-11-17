namespace CinemaGo.Application.DTOs.Tickets;

public class TicketResponseDto
{
    public int Id { get; set; }

    public int ReservationId { get; set; }

    public string TicketNumber { get; set; } = string.Empty;

    public string TicketType { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public DateTime IssuedAt { get; set; }
}