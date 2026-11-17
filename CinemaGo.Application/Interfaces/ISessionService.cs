using CinemaGo.Application.DTOs.Sessions;

namespace CinemaGo.Application.Interfaces;

public interface ISessionService
{
    Task<List<SessionResponseDto>> GetAllAsync();

    Task<SessionResponseDto?> GetByIdAsync(int id);

    Task<List<SessionResponseDto>> GetByMovieIdAsync(int movieId);

    Task<List<SessionResponseDto>> GetByHallIdAsync(int hallId);

    Task<List<SessionResponseDto>> GetFilteredAsync(
        int? movieId,
        int? hallId,
        DateTime? date,
        decimal? minPrice,
        decimal? maxPrice,
        bool? availableSeatsOnly);

    Task<List<SessionSeatResponseDto>> GetSessionSeatsAsync(
        int sessionId);

    Task<SessionResponseDto> CreateAsync(CreateSessionDto request);

    Task<bool> UpdateAsync(int id, UpdateSessionDto request);

    Task<bool> DeleteAsync(int id);
}