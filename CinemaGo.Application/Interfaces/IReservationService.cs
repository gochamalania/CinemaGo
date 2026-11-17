using CinemaGo.Application.DTOs.Reservations;

namespace CinemaGo.Application.Interfaces;

public interface IReservationService
{
    Task<ReservationResponseDto> CreateAsync(
        int userId,
        CreateReservationDto dto);

    Task<ReservationResponseDto?> GetByIdAsync(
        int reservationId,
        int userId);

    Task<List<ReservationResponseDto>> GetMyReservationsAsync(
        int userId);

    Task CancelAsync(
        int reservationId,
        int userId);

    Task<ReservationResponseDto> ConfirmAsync(
        int reservationId,
        int userId);
}