using CinemaGo.Application.DTOs.Halls;

namespace CinemaGo.Application.Interfaces;

public interface IHallService
{
    Task<List<HallResponseDto>> GetAllAsync();

    Task<HallResponseDto?> GetByIdAsync(int id);

    Task<HallResponseDto> CreateAsync(CreateHallDto request);

    Task<bool> UpdateAsync(int id, UpdateHallDto request);

    Task<bool> DeleteAsync(int id);
}