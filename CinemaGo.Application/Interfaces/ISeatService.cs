using CinemaGo.Application.DTOs.Seats;

namespace CinemaGo.Application.Interfaces;

public interface ISeatService
{
    Task<List<SeatResponseDto>> GetAllAsync();

    Task<SeatResponseDto?> GetByIdAsync(int id);

    Task<List<SeatResponseDto>> GetByHallIdAsync(int hallId);

    Task<SeatResponseDto> CreateAsync(CreateSeatDto request);

    Task<bool> UpdateAsync(int id, UpdateSeatDto request);

    Task<bool> DeleteAsync(int id);
}