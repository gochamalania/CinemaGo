using CinemaGo.Domain.Entities;

namespace CinemaGo.Application.Interfaces;

public interface ISeatRepository
{
    Task<List<Seat>> GetAllAsync();

    Task<Seat?> GetByIdAsync(int id);

    Task<List<Seat>> GetByHallIdAsync(int hallId);

    Task<Seat?> GetByPositionAsync(
        int hallId,
        int rowNumber,
        int seatNumber);

    Task<int> CountByHallIdAsync(int hallId);

    Task AddAsync(Seat seat);

    Task UpdateAsync(Seat seat);

    Task DeleteAsync(Seat seat);
    
    
    Task<bool> HasReservationsAsync(int seatId);

}