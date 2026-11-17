
using CinemaGo.Application.DTOs.Sessions;
using CinemaGo.Application.Interfaces;
using CinemaGo.Domain.Entities;

namespace CinemaGo.Application.Services;

public class SessionService : ISessionService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IMovieRepository _movieRepository;
    private readonly IHallRepository _hallRepository;
    private readonly ISeatRepository _seatRepository;
    private readonly IReservationRepository _reservationRepository;

    public SessionService(
        ISessionRepository sessionRepository,
        IMovieRepository movieRepository,
        IHallRepository hallRepository,
        ISeatRepository seatRepository,
        IReservationRepository reservationRepository)
    {
        _sessionRepository = sessionRepository;
        _movieRepository = movieRepository;
        _hallRepository = hallRepository;
        _seatRepository = seatRepository;
        _reservationRepository = reservationRepository;
    }

    public async Task<List<SessionResponseDto>> GetAllAsync()
    {
        var sessions = await _sessionRepository.GetAllAsync();

        return sessions.Select(MapToDto).ToList();
    }

    public async Task<SessionResponseDto?> GetByIdAsync(int id)
    {
        var session = await _sessionRepository.GetByIdAsync(id);

        return session == null ? null : MapToDto(session);
    }

    public async Task<List<SessionResponseDto>> GetByMovieIdAsync(
        int movieId)
    {
        var movie = await _movieRepository.GetByIdAsync(movieId);

        if (movie == null)
        {
            throw new KeyNotFoundException("Movie not found.");
        }

        var sessions = await _sessionRepository
            .GetByMovieIdAsync(movieId);

        return sessions.Select(MapToDto).ToList();
    }

    public async Task<List<SessionResponseDto>> GetByHallIdAsync(
        int hallId)
    {
        var hall = await _hallRepository.GetByIdAsync(hallId);

        if (hall == null)
        {
            throw new KeyNotFoundException("Hall not found.");
        }

        var sessions = await _sessionRepository
            .GetByHallIdAsync(hallId);

        return sessions.Select(MapToDto).ToList();
    }

    public async Task<List<SessionSeatResponseDto>> GetSessionSeatsAsync(
        int sessionId)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);

        if (session == null)
        {
            throw new KeyNotFoundException("Session not found.");
        }

        var seats = await _seatRepository.GetByHallIdAsync(session.HallId);

        var reservations = await _reservationRepository
            .GetBySessionIdAsync(sessionId);

        var now = DateTime.UtcNow;

        var reservedSeatIds = reservations
            .Where(r => r.Status == "Confirmed")
            .Select(r => r.SeatId)
            .ToHashSet();

        var heldSeatIds = reservations
            .Where(r =>
                r.Status == "Held" &&
                r.ExpiresAt.HasValue &&
                r.ExpiresAt.Value > now)
            .Select(r => r.SeatId)
            .ToHashSet();

        return seats
            .OrderBy(s => s.RowNumber)
            .ThenBy(s => s.SeatNumber)
            .Select(seat => new SessionSeatResponseDto
            {
                SeatId = seat.Id,
                RowNumber = seat.RowNumber,
                SeatNumber = seat.SeatNumber,
                IsVip = seat.IsVip,
                Status = reservedSeatIds.Contains(seat.Id)
                    ? "Reserved"
                    : heldSeatIds.Contains(seat.Id)
                        ? "Held"
                        : "Available"
            })
            .ToList();
    }

    public async Task<List<SessionResponseDto>> GetFilteredAsync(
        int? movieId,
        int? hallId,
        DateTime? date,
        decimal? minPrice,
        decimal? maxPrice,
        bool? availableSeatsOnly)
    {
        if (movieId.HasValue && movieId.Value <= 0)
        {
            throw new ArgumentException(
                "Movie ID must be greater than zero.");
        }

        if (hallId.HasValue && hallId.Value <= 0)
        {
            throw new ArgumentException(
                "Hall ID must be greater than zero.");
        }

        if (minPrice.HasValue && minPrice.Value < 0)
        {
            throw new ArgumentException(
                "Minimum price cannot be negative.");
        }

        if (maxPrice.HasValue && maxPrice.Value < 0)
        {
            throw new ArgumentException(
                "Maximum price cannot be negative.");
        }

        if (minPrice.HasValue &&
            maxPrice.HasValue &&
            minPrice.Value > maxPrice.Value)
        {
            throw new ArgumentException(
                "Minimum price cannot exceed maximum price.");
        }

        var sessions = await _sessionRepository.GetFilteredAsync(
            movieId,
            hallId,
            date,
            minPrice,
            maxPrice,
            availableSeatsOnly);

        return sessions.Select(MapToDto).ToList();
    }

    public async Task<SessionResponseDto> CreateAsync(
        CreateSessionDto request)
    {
        await ValidateSessionAsync(
            request.MovieId,
            request.HallId,
            request.StartTime,
            request.BasePrice);

        var movie = await _movieRepository.GetByIdAsync(
            request.MovieId);

        if (movie == null)
        {
            throw new KeyNotFoundException("Movie not found.");
        }

        var session = new Session
        {
            MovieId = request.MovieId,
            HallId = request.HallId,
            StartTime = request.StartTime,
            BasePrice = request.BasePrice
        };

        await _sessionRepository.AddWithOverlapProtectionAsync(
            session,
            movie.DurationMinutes);

        return MapToDto(session);
    }

    public async Task<bool> UpdateAsync(
        int id,
        UpdateSessionDto request)
    {
        var session = await _sessionRepository.GetByIdAsync(id);

        if (session == null)
        {
            return false;
        }

        var hasReservations = await _sessionRepository
            .HasReservationsAsync(id);

        if (hasReservations)
        {
            throw new InvalidOperationException(
                "Cannot update a session that has reservations.");
        }

        await ValidateSessionAsync(
            request.MovieId,
            request.HallId,
            request.StartTime,
            request.BasePrice,
            id);

        var movie = await _movieRepository.GetByIdAsync(
            request.MovieId);

        if (movie == null)
        {
            throw new KeyNotFoundException("Movie not found.");
        }

        session.MovieId = request.MovieId;
        session.HallId = request.HallId;
        session.StartTime = request.StartTime;
        session.BasePrice = request.BasePrice;

        await _sessionRepository.UpdateWithOverlapProtectionAsync(
            session,
            movie.DurationMinutes);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var session = await _sessionRepository.GetByIdAsync(id);

        if (session == null)
        {
            return false;
        }

        var hasReservations = await _sessionRepository
            .HasReservationsAsync(id);

        if (hasReservations)
        {
            throw new InvalidOperationException(
                "Cannot delete a session that has reservations.");
        }

        await _sessionRepository.DeleteAsync(session);

        return true;
    }

    private async Task ValidateSessionAsync(
        int movieId,
        int hallId,
        DateTime startTime,
        decimal basePrice,
        int? excludeSessionId = null)
    {
        var movie = await _movieRepository.GetByIdAsync(movieId);

        if (movie == null)
        {
            throw new KeyNotFoundException("Movie not found.");
        }

        var hall = await _hallRepository.GetByIdAsync(hallId);

        if (hall == null)
        {
            throw new KeyNotFoundException("Hall not found.");
        }

        if (startTime <= DateTime.UtcNow)
        {
            throw new ArgumentException(
                "Session start time must be in the future.");
        }

        if (basePrice <= 0)
        {
            throw new ArgumentException(
                "Base price must be greater than zero.");
        }

        if (movie.DurationMinutes <= 0)
        {
            throw new ArgumentException(
                "Movie duration must be greater than zero.");
        }

        var endTime = startTime.AddMinutes(
            movie.DurationMinutes);

        var hasOverlap = await _sessionRepository
            .HasOverlappingSessionAsync(
                hallId,
                startTime,
                endTime,
                excludeSessionId);

        if (hasOverlap)
        {
            throw new InvalidOperationException(
                "Another session is already scheduled in this hall.");
        }
    }

    private static SessionResponseDto MapToDto(Session session)
    {
        return new SessionResponseDto
        {
            Id = session.Id,
            MovieId = session.MovieId,
            HallId = session.HallId,
            StartTime = session.StartTime,
            BasePrice = session.BasePrice
        };
    }
}
