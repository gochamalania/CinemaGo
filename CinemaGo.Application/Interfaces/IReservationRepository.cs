using CinemaGo.Domain.Entities;

namespace CinemaGo.Application.Interfaces;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(int id);

    Task<List<Reservation>> GetByUserIdAsync(int userId);

    Task<List<Reservation>> GetBySessionIdAsync(int sessionId);

    Task<bool> IsSeatReservedAsync(
        int sessionId,
        int seatId);

    Task AddAsync(Reservation reservation);

    Task UpdateAsync(Reservation reservation);

    Task<bool> TryCreateHoldAsync(
        Reservation reservation);

    Task<Reservation?> TryConfirmAsync(
        int reservationId,
        int userId);

    Task<bool> TryCancelAsync(
        int reservationId,
        int userId);
}