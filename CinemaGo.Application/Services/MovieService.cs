
using CinemaGo.Application.DTOs.Movies;
using CinemaGo.Application.Interfaces;
using CinemaGo.Domain.Entities;

namespace CinemaGo.Application.Services;

public class MovieService : IMovieService
{
    private readonly IMovieRepository _movieRepository;

    public MovieService(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public async Task<List<MovieResponseDto>> GetAllAsync()
    {
        var movies = await _movieRepository.GetAllAsync();

        return movies.Select(MapToDto).ToList();
    }

    public async Task<MovieResponseDto?> GetByIdAsync(int id)
    {
        var movie = await _movieRepository.GetByIdAsync(id);

        return movie == null ? null : MapToDto(movie);
    }

    public async Task<MovieResponseDto> CreateAsync(CreateMovieDto request)
    {
        ValidateAgeRestriction(request.AgeRestriction);

        var movie = new Movie
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            DurationMinutes = request.DurationMinutes,
            ReleaseDate = request.ReleaseDate,
            AgeRestriction = request.AgeRestriction
        };

        await _movieRepository.AddAsync(movie);

        return MapToDto(movie);
    }

    public async Task<bool> UpdateAsync(int id, UpdateMovieDto request)
    {
        var movie = await _movieRepository.GetByIdAsync(id);

        if (movie == null)
            return false;

        ValidateAgeRestriction(request.AgeRestriction);

        movie.Title = request.Title.Trim();
        movie.Description = request.Description.Trim();
        movie.DurationMinutes = request.DurationMinutes;
        movie.ReleaseDate = request.ReleaseDate;
        movie.AgeRestriction = request.AgeRestriction;

        await _movieRepository.UpdateAsync(movie);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var movie = await _movieRepository.GetByIdAsync(id);

        if (movie == null)
            return false;

        await _movieRepository.DeleteAsync(movie);

        return true;
    }

    private static void ValidateAgeRestriction(int ageRestriction)
    {
        if (ageRestriction is not (0 or 12 or 16 or 18))
        {
            throw new ArgumentException(
                "Age restriction must be 0, 12, 16, or 18.");
        }
    }

    private static MovieResponseDto MapToDto(Movie movie)
    {
        return new MovieResponseDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Description = movie.Description,
            DurationMinutes = movie.DurationMinutes,
            ReleaseDate = movie.ReleaseDate,
            AgeRestriction = movie.AgeRestriction
        };
    }
}
