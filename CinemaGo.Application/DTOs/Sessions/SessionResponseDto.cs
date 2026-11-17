namespace CinemaGo.Application.DTOs.Sessions;

public class SessionResponseDto
{
    public int Id { get; set; }

    public int MovieId { get; set; }

    public int HallId { get; set; }

    public DateTime StartTime { get; set; }

    public decimal BasePrice { get; set; }
}