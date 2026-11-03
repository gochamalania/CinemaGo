namespace CinemaGo.Domain.Entities;

public class Seat
{
    public int  Id { get; set; }
    public int RowNumber { get; set; }
    public int SeatNumber { get; set; }
    public bool IsVip { get; set; }
    public int HallId { get; set; }
    public Hall Hall { get; set; } = null!;
}