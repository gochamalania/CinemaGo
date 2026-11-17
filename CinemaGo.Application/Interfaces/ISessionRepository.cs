using CinemaGo.Domain.Entities;

namespace CinemaGo.Application.Interfaces;

public interface ISessionRepository
{
    Task<List<Session>> GetAllAsync();

    Task<Session?> GetByIdAsync(int id);

    Task<List<Session>> GetByMovieIdAsync(int movieId);

    Task<List<Session>> GetByHallIdAsync(int hallId);

    Task<List<Session>> GetFilteredAsync(
        int? movieId,
        int? hallId,
        DateTime? date,
        decimal? minPrice,
        decimal? maxPrice,
        bool? availableSeatsOnly);

    Task AddAsync(Session session);

    Task UpdateAsync(Session session);

    Task DeleteAsync(Session session);

    Task<bool> HasReservationsAsync(int sessionId);

    Task<bool> HasOverlappingSessionAsync(
        int hallId,
        DateTime startTime,
        DateTime endTime,
        int? excludeSessionId = null);

    Task AddWithOverlapProtectionAsync(
        Session session,
        int durationMinutes);

    Task UpdateWithOverlapProtectionAsync(
        Session session,
        int durationMinutes);
}