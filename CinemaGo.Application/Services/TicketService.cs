
using CinemaGo.Application.DTOs.Tickets;
using CinemaGo.Application.Interfaces;
using CinemaGo.Domain.Entities;

namespace CinemaGo.Application.Services;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMovieRepository _movieRepository;

    public TicketService(
        ITicketRepository ticketRepository,
        IReservationRepository reservationRepository,
        ISessionRepository sessionRepository,
        IUserRepository userRepository,
        IMovieRepository movieRepository)
    {
        _ticketRepository = ticketRepository;
        _reservationRepository = reservationRepository;
        _sessionRepository = sessionRepository;
        _userRepository = userRepository;
        _movieRepository = movieRepository;
    }

    public async Task<TicketResponseDto> CreateAsync(
        int userId,
        CreateTicketDto dto)
    {
        if (userId <= 0)
            throw new UnauthorizedAccessException(
                "Invalid user.");

        if (dto == null || dto.ReservationId <= 0)
            throw new ArgumentException(
                "Invalid reservation.");

        // Validate ticket type
        var ticketType = dto.TicketType?.Trim();

        if (string.IsNullOrWhiteSpace(ticketType))
            throw new ArgumentException(
                "Ticket type is required.");

        ticketType = ticketType.ToLowerInvariant() switch
        {
            "adult" => "Adult",
            "child" => "Child",
            "student" => "Student",
            _ => throw new ArgumentException(
                "Invalid ticket type.")
        };

        // Find reservation
        var reservation = await _reservationRepository
            .GetByIdAsync(dto.ReservationId);

        if (reservation == null)
            throw new KeyNotFoundException(
                "Reservation not found.");

        // Check reservation owner
        if (reservation.UserId != userId)
            throw new UnauthorizedAccessException(
                "You cannot issue a ticket for this reservation.");

        // Only confirmed reservations
        if (reservation.Status != "Confirmed")
            throw new InvalidOperationException(
                "Only confirmed reservations can receive tickets.");

        // Check duplicate ticket
        var existingTicket = await _ticketRepository
            .GetByReservationIdAsync(reservation.Id);

        if (existingTicket != null)
            throw new InvalidOperationException(
                "Ticket already exists for this reservation.");

        // Get session
        var session = await _sessionRepository
            .GetByIdAsync(reservation.SessionId);

        if (session == null)
            throw new KeyNotFoundException(
                "Session not found.");

        if (session.StartTime <= DateTime.UtcNow)
            throw new InvalidOperationException(
                "Cannot issue tickets for a started session.");

        // Get movie
        var movie = await _movieRepository
            .GetByIdAsync(session.MovieId);

        if (movie == null)
            throw new KeyNotFoundException(
                "Movie not found.");

        // Get user
        var user = await _userRepository
            .GetByIdAsync(userId);

        if (user == null)
            throw new UnauthorizedAccessException(
                "User not found.");

        // Validate age restriction
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
                throw new System.Security.SecurityException(
                    $"You must be at least {movie.AgeRestriction} years old to watch this movie.");
        }

        // Child tickets are not allowed for 16+ and 18+ movies
        if (ticketType == "Child" && movie.AgeRestriction >= 16)
            throw new ArgumentException(
                "Child tickets are not allowed for movies rated 16+ or 18+.");

        // Calculate ticket price
        decimal price = ticketType switch
        {
            "Adult" => session.BasePrice,
            "Child" => session.BasePrice * 0.60m,
            "Student" => session.BasePrice * 0.75m,
            _ => throw new ArgumentException(
                "Invalid ticket type.")
        };

        price = Math.Round(
            price,
            2,
            MidpointRounding.AwayFromZero);

        // Generate ticket number
        var ticketNumber =
            $"CG-{Guid.NewGuid():N}".ToUpperInvariant();

        // Create ticket
        var ticket = new Ticket
        {
            ReservationId = reservation.Id,
            TicketNumber = ticketNumber,
            TicketType = ticketType,
            Price = price,
            IssuedAt = DateTime.UtcNow
        };

        // Save ticket
        await _ticketRepository.AddAsync(ticket);

        return MapToDto(ticket);
    }

    public async Task<TicketResponseDto?> GetByIdAsync(
        int ticketId,
        int userId)
    {
        var ticket = await _ticketRepository
            .GetByIdAsync(ticketId);

        if (ticket == null ||
            ticket.Reservation.UserId != userId)
            return null;

        return MapToDto(ticket);
    }

    public async Task<List<TicketResponseDto>> GetMyTicketsAsync(
        int userId)
    {
        var tickets = await _ticketRepository
            .GetByUserIdAsync(userId);

        return tickets
            .Select(MapToDto)
            .ToList();
    }

    private static TicketResponseDto MapToDto(Ticket ticket)
    {
        return new TicketResponseDto
        {
            Id = ticket.Id,
            ReservationId = ticket.ReservationId,
            TicketNumber = ticket.TicketNumber,
            TicketType = ticket.TicketType,
            Price = ticket.Price,
            IssuedAt = ticket.IssuedAt
        };
    }
}
