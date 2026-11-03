namespace CinemaGo.Domain.Entities;

public class Ticket
{
    public int Id { get; set; }
    
    public int ReservationId { get; set; }
    public Reservation Reservation { get; set; } = null!;
    
    public string TicketNumber { get; set; } = string.Empty;
    
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
}