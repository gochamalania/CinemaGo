namespace CinemaGo.Application.DTOs.Sessions;

public class SessionSeatResponseDto
{
    public int SeatId { get; set; }

    public int RowNumber { get; set; }

    public int SeatNumber { get; set; }

    public bool IsVip { get; set; }

    public string Status { get; set; } = "Available";
}