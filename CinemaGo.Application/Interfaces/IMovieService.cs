using CinemaGo.Application.DTOs.Movies;

namespace CinemaGo.Application.Interfaces;

public interface IMovieService
{
    Task<List<MovieResponseDto>> GetAllAsync();
    Task<MovieResponseDto?> GetByIdAsync(int id);
    Task<MovieResponseDto> CreateAsync(CreateMovieDto request);
    Task<bool> UpdateAsync(int id, UpdateMovieDto request);
    Task<bool> DeleteAsync(int id);
}