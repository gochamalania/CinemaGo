namespace CinemaGo.Domain.Entities;

public class Reservation
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public int SessionId { get; set; }
    public Session Session { get; set; } = null!;
    
    public int SeatId { get; set; }
    public Seat Seat { get; set; } = null!;

    public DateTime ReserveAt { get; set; } = DateTime.UtcNow;
    
    public bool IsCancelled { get; set; } = false;
}