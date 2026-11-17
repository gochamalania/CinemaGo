using System.ComponentModel.DataAnnotations;

namespace CinemaGo.Application.DTOs.Seats;

public class CreateSeatDto
{
    [Range(1, 200)]
    public int RowNumber { get; set; }

    [Range(1, 200)]
    public int SeatNumber { get; set; }

    public bool IsVip { get; set; }

    [Range(1, int.MaxValue)]
    public int HallId { get; set; }
}