
using System.Security;
using CinemaGo.Application.DTOs.Reservations;
using CinemaGo.Application.Interfaces;
using CinemaGo.Domain.Entities;

namespace CinemaGo.Application.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly ISeatRepository _seatRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMovieRepository _movieRepository;

    public ReservationService(
        IReservationRepository reservationRepository,
        ISessionRepository sessionRepository,
        ISeatRepository seatRepository,
        IUserRepository userRepository,
        IMovieRepository movieRepository)
    {
        _reservationRepository = reservationRepository;
        _sessionRepository = sessionRepository;
        _seatRepository = seatRepository;
        _userRepository = userRepository;
        _movieRepository = movieRepository;
    }

    public async Task<ReservationResponseDto?> GetByIdAsync(
        int reservationId,
        int userId)
    {
        var reservation =
            await _reservationRepository.GetByIdAsync(reservationId);

        if (reservation == null || reservation.UserId != userId)
            return null;

        return MapToDto(reservation);
    }

    public async Task<List<ReservationResponseDto>>
        GetMyReservationsAsync(int userId)
    {
        var reservations =
            await _reservationRepository.GetByUserIdAsync(userId);

        return reservations
            .Select(MapToDto)
            .ToList();
    }

    public async Task<ReservationResponseDto> CreateAsync(
        int userId,
        CreateReservationDto dto)
    {
        if (userId <= 0)
            throw new ArgumentException("Invalid user.");

        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        if (dto.SessionId <= 0 || dto.SeatId <= 0)
            throw new ArgumentException(
                "SessionId and SeatId must be positive.");

        var session = await _sessionRepository
            .GetByIdAsync(dto.SessionId);

        if (session == null)
            throw new KeyNotFoundException(
                "Session not found.");

        if (session.StartTime <= DateTime.UtcNow)
            throw new InvalidOperationException(
                "Cannot reserve a seat for a started session.");

        // Get movie and its age restriction
        var movie = await _movieRepository
            .GetByIdAsync(session.MovieId);

        if (movie == null)
            throw new KeyNotFoundException(
                "Movie not found.");

        // Get customer
        var user = await _userRepository
            .GetByIdAsync(userId);

        if (user == null)
            throw new UnauthorizedAccessException(
                "User not found.");

        // Check age restriction before reserving the seat
        if (movie.AgeRestriction > 0)
        {
            if (!user.DateOfBirth.HasValue)
                throw new ArgumentException(
                    "Date of birth is required for age-restricted movies.");

            var today = DateTime.UtcNow.Date;
            var dateOfBirth = user.DateOfBirth.Value.Date;

            if (dateOfBirth > today)
                throw new ArgumentException(
                    "Invalid date of birth.");

            var age = today.Year - dateOfBirth.Year;

            if (dateOfBirth > today.AddYears(-age))
                age--;

            if (age < movie.AgeRestriction)
                throw new SecurityException(
                    $"You must be at least {movie.AgeRestriction} years old to watch this movie.");
        }

        // Check seat
        var seat = await _seatRepository
            .GetByIdAsync(dto.SeatId);

        if (seat == null)
            throw new KeyNotFoundException(
                "Seat not found.");

        if (seat.HallId != session.HallId)
            throw new ArgumentException(
                "Seat does not belong to the session hall.");

        // Create reservation
        var reservation = new Reservation
        {
            UserId = userId,
            SessionId = dto.SessionId,
            SeatId = dto.SeatId
        };

        // Existing hold and concurrency protection
        var created = await _reservationRepository
            .TryCreateHoldAsync(reservation);

        if (!created)
            throw new InvalidOperationException(
                "Seat is already reserved.");

        return MapToDto(reservation);
    }

    public async Task CancelAsync(
        int reservationId,
        int userId)
    {
        if (reservationId <= 0 || userId <= 0)
            throw new ArgumentException(
                "Invalid reservation or user.");

        await _reservationRepository.TryCancelAsync(
            reservationId,
            userId);
    }

    public async Task<ReservationResponseDto> ConfirmAsync(
        int reservationId,
        int userId)
    {
        if (reservationId <= 0 || userId <= 0)
            throw new ArgumentException(
                "Invalid reservation or user.");

        var reservation = await _reservationRepository
            .TryConfirmAsync(reservationId, userId);

        if (reservation == null)
            throw new KeyNotFoundException(
                "Reservation not found.");

        return MapToDto(reservation);
    }

    private static ReservationResponseDto MapToDto(
        Reservation reservation)
    {
        return new ReservationResponseDto
        {
            Id = reservation.Id,
            UserId = reservation.UserId,
            SessionId = reservation.SessionId,
            SeatId = reservation.SeatId,
            Status = reservation.Status,
            CreatedAt = reservation.CreatedAt,
            ExpiresAt = reservation.ExpiresAt
        };
    }
}
