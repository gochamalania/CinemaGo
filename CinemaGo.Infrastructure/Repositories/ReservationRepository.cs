
using System.Data;
using CinemaGo.Application.Interfaces;
using CinemaGo.Domain.Entities;
using CinemaGo.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CinemaGo.Infrastructure.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly CinemaGoDbContext _context;

    public ReservationRepository(CinemaGoDbContext context)
    {
        _context = context;
    }

    public async Task<Reservation?> GetByIdAsync(int id)
    {
        return await _context.Reservations
            .AsNoTracking()
            .Include(r => r.Session)
            .Include(r => r.Seat)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<Reservation>> GetByUserIdAsync(int userId)
    {
        return await _context.Reservations
            .AsNoTracking()
            .Include(r => r.Session)
            .Include(r => r.Seat)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Reservation>> GetBySessionIdAsync(int sessionId)
    {
        return await _context.Reservations
            .AsNoTracking()
            .Include(r => r.Seat)
            .Where(r => r.SessionId == sessionId)
            .ToListAsync();
    }

    public async Task<bool> IsSeatReservedAsync(
        int sessionId,
        int seatId)
    {
        var now = DateTime.UtcNow;

        return await _context.Reservations.AnyAsync(r =>
            r.SessionId == sessionId &&
            r.SeatId == seatId &&
            (
                r.Status == "Confirmed" ||
                (
                    r.Status == "Held" &&
                    r.ExpiresAt.HasValue &&
                    r.ExpiresAt.Value > now
                )
            ));
    }

    // Keep for compatibility with the existing interface.
    // Do not use this method to create active reservations.
    public Task AddAsync(Reservation reservation)
    {
        throw new NotSupportedException(
            "Use TryCreateHoldAsync to create reservations.");
    }

    // Prevent updates that bypass reservation locking.
    public Task UpdateAsync(Reservation reservation)
    {
        throw new NotSupportedException(
            "Use TryConfirmAsync or TryCancelAsync.");
    }

    // Automatically expire reservation holds.
    public async Task<int> ExpireOldHoldsAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var expiredHolds = await _context.Reservations
            .AsNoTracking()
            .Where(r =>
                r.Status == "Held" &&
                r.ExpiresAt.HasValue &&
                r.ExpiresAt.Value <= now)
            .Select(r => new
            {
                r.Id,
                r.SessionId,
                r.SeatId
            })
            .ToListAsync(cancellationToken);

        var expiredCount = 0;

        foreach (var hold in expiredHolds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.ReadCommitted,
                    cancellationToken);

            await AcquireSeatLockAsync(
                hold.SessionId,
                hold.SeatId,
                transaction);

            var affectedRows = await _context.Reservations
                .Where(r =>
                    r.Id == hold.Id &&
                    r.Status == "Held" &&
                    r.ExpiresAt.HasValue &&
                    r.ExpiresAt.Value <= DateTime.UtcNow)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(r => r.Status, "Expired"),
                    cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            expiredCount += affectedRows;
        }

        return expiredCount;
    }

    private async Task AcquireSeatLockAsync(
        int sessionId,
        int seatId,
        IDbContextTransaction transaction)
    {
        var lockName = $"CinemaGo:Seat:{sessionId}:{seatId}";

        var connection =
            (SqlConnection)_context.Database.GetDbConnection();

        await using var command = new SqlCommand(
            """
            DECLARE @result int;

            EXEC @result = sp_getapplock
                @Resource = @Resource,
                @LockMode = 'Exclusive',
                @LockOwner = 'Transaction',
                @LockTimeout = 10000;

            SELECT @result;
            """,
            connection,
            (SqlTransaction)transaction.GetDbTransaction());

        command.Parameters.Add("@Resource", SqlDbType.NVarChar, 255)
            .Value = lockName;

        var result = Convert.ToInt32(
            await command.ExecuteScalarAsync());

        if (result < 0)
        {
            throw new InvalidOperationException(
                "Could not acquire reservation lock.");
        }
    }

    public async Task<bool> TryCreateHoldAsync(
        Reservation reservation)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted);

        await AcquireSeatLockAsync(
            reservation.SessionId,
            reservation.SeatId,
            transaction);

        var now = DateTime.UtcNow;

        var seatReserved = await _context.Reservations.AnyAsync(r =>
            r.SessionId == reservation.SessionId &&
            r.SeatId == reservation.SeatId &&
            (
                r.Status == "Confirmed" ||
                (
                    r.Status == "Held" &&
                    r.ExpiresAt.HasValue &&
                    r.ExpiresAt.Value > now
                )
            ));

        if (seatReserved)
        {
            return false;
        }

        // Recheck session and seat within the transaction.
        var session = await _context.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s =>
                s.Id == reservation.SessionId);

        if (session == null)
            throw new KeyNotFoundException(
                "Session not found.");

        if (session.StartTime <= now)
            throw new InvalidOperationException(
                "Cannot reserve a seat for a started session.");

        var seat = await _context.Seats
            .AsNoTracking()
            .FirstOrDefaultAsync(s =>
                s.Id == reservation.SeatId);

        if (seat == null)
            throw new KeyNotFoundException(
                "Seat not found.");

        if (seat.HallId != session.HallId)
            throw new ArgumentException(
                "Seat does not belong to the session hall.");

        reservation.Status = "Held";
        reservation.CreatedAt = now;
        reservation.ExpiresAt = now.AddMinutes(8);

        await _context.Reservations.AddAsync(reservation);
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        return true;
    }

    public async Task<Reservation?> TryConfirmAsync(
        int reservationId,
        int userId)
    {
        // Read identifiers without tracking.
        var reservationInfo = await _context.Reservations
            .AsNoTracking()
            .Where(r =>
                r.Id == reservationId &&
                r.UserId == userId)
            .Select(r => new
            {
                r.SessionId,
                r.SeatId
            })
            .FirstOrDefaultAsync();

        if (reservationInfo == null)
            throw new KeyNotFoundException(
                "Reservation not found.");

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted);

        await AcquireSeatLockAsync(
            reservationInfo.SessionId,
            reservationInfo.SeatId,
            transaction);

        // Read fresh reservation data after acquiring lock.
        var reservation = await _context.Reservations
            .Include(r => r.Session)
            .FirstOrDefaultAsync(r =>
                r.Id == reservationId &&
                r.UserId == userId);

        if (reservation == null)
            throw new KeyNotFoundException(
                "Reservation not found.");

        var now = DateTime.UtcNow;

        if (reservation.Status != "Held")
            throw new InvalidOperationException(
                "Only held reservations can be confirmed.");

        if (!reservation.ExpiresAt.HasValue ||
            reservation.ExpiresAt.Value <= now)
            throw new InvalidOperationException(
                "Reservation hold has expired.");

        if (reservation.Session.StartTime <= now)
            throw new InvalidOperationException(
                "Cannot confirm a reservation after the session has started.");

        var anotherActiveReservation =
            await _context.Reservations.AnyAsync(r =>
                r.Id != reservation.Id &&
                r.SessionId == reservation.SessionId &&
                r.SeatId == reservation.SeatId &&
                (
                    r.Status == "Confirmed" ||
                    (
                        r.Status == "Held" &&
                        r.ExpiresAt.HasValue &&
                        r.ExpiresAt.Value > now
                    )
                ));

        if (anotherActiveReservation)
            throw new InvalidOperationException(
                "Seat is already reserved by another user.");
        
        
        // Recheck movie age restriction before confirmation.
        var movie = await _context.Movies
            .AsNoTracking()
            .FirstOrDefaultAsync(m =>
                m.Id == reservation.Session.MovieId);

        if (movie == null)
            throw new KeyNotFoundException(
                "Movie not found.");

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                u.Id == userId);

        if (user == null)
            throw new UnauthorizedAccessException(
                "User not found.");

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


        reservation.Status = "Confirmed";
        reservation.ExpiresAt = null;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return reservation;
    }

    public async Task<bool> TryCancelAsync(
        int reservationId,
        int userId)
    {
        var reservationInfo = await _context.Reservations
            .AsNoTracking()
            .Where(r =>
                r.Id == reservationId &&
                r.UserId == userId)
            .Select(r => new
            {
                r.SessionId,
                r.SeatId
            })
            .FirstOrDefaultAsync();

        if (reservationInfo == null)
            throw new KeyNotFoundException(
                "Reservation not found.");

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted);

        // Lock the seat first.
        await AcquireSeatLockAsync(
            reservationInfo.SessionId,
            reservationInfo.SeatId,
            transaction);

        // Use the same ticket lock as TicketRepository.
        var connection =
            (SqlConnection)_context.Database.GetDbConnection();

        await using (var command = new SqlCommand(
                         """
                         DECLARE @result int;

                         EXEC @result = sp_getapplock
                             @Resource = @Resource,
                             @LockMode = 'Exclusive',
                             @LockOwner = 'Transaction',
                             @LockTimeout = 10000;

                         SELECT @result;
                         """,
                         connection,
                         (SqlTransaction)transaction.GetDbTransaction()))
        {
            command.Parameters.Add(
                    "@Resource",
                    SqlDbType.NVarChar,
                    255
                ).Value =
                $"CinemaGo:Ticket:Reservation:{reservationId}";

            var result = Convert.ToInt32(
                await command.ExecuteScalarAsync());

            if (result < 0)
                throw new InvalidOperationException(
                    "Could not acquire ticket lock.");
        }

        // Re-read reservation after acquiring locks.
        var reservation = await _context.Reservations
            .Include(r => r.Session)
            .FirstOrDefaultAsync(r =>
                r.Id == reservationId &&
                r.UserId == userId);

        if (reservation == null)
            throw new KeyNotFoundException(
                "Reservation not found.");

        if (reservation.Status == "Cancelled")
            throw new InvalidOperationException(
                "Reservation is already cancelled.");

        if (reservation.Status != "Held" &&
            reservation.Status != "Confirmed")
            throw new InvalidOperationException(
                "Reservation cannot be cancelled.");

        // NEW: Prevent cancellation of an expired hold.
        if (reservation.Status == "Held" &&
            (!reservation.ExpiresAt.HasValue ||
             reservation.ExpiresAt.Value <= DateTime.UtcNow))
        {
            throw new InvalidOperationException(
                "Reservation hold has expired.");
        }

        if (reservation.Session.StartTime <= DateTime.UtcNow)
            throw new InvalidOperationException(
                "Cannot cancel a reservation after the session has started.");

        // Do not cancel a reservation with an issued ticket.
        var ticketExists = await _context.Tickets
            .AnyAsync(t =>
                t.ReservationId == reservationId);

        if (ticketExists)
            throw new InvalidOperationException(
                "Cannot cancel a reservation with an issued ticket.");

        reservation.Status = "Cancelled";
        reservation.ExpiresAt = null;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return true;
    }
}
