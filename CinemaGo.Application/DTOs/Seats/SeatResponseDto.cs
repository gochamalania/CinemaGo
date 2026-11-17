namespace CinemaGo.Application.DTOs.Seats;

public class SeatResponseDto
{
    public int Id { get; set; }

    public int RowNumber { get; set; }

    public int SeatNumber { get; set; }

    public bool IsVip { get; set; }

    public int HallId { get; set; }
}