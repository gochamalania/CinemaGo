
using System.Data;
using CinemaGo.Application.Interfaces;
using CinemaGo.Domain.Entities;
using CinemaGo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CinemaGo.Infrastructure.Repositories;

public class SessionRepository : ISessionRepository
{
    private readonly CinemaGoDbContext _context;

    public SessionRepository(CinemaGoDbContext context)
    {
        _context = context;
    }

    public async Task<List<Session>> GetAllAsync()
    {
        return await _context.Sessions
            .AsNoTracking()
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<Session?> GetByIdAsync(int id)
    {
        return await _context.Sessions
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<Session>> GetByMovieIdAsync(int movieId)
    {
        return await _context.Sessions
            .AsNoTracking()
            .Where(s => s.MovieId == movieId)
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<List<Session>> GetByHallIdAsync(int hallId)
    {
        return await _context.Sessions
            .AsNoTracking()
            .Where(s => s.HallId == hallId)
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<List<Session>> GetFilteredAsync(
        int? movieId,
        int? hallId,
        DateTime? date,
        decimal? minPrice,
        decimal? maxPrice,
        bool? availableSeatsOnly)
    {
        IQueryable<Session> query = _context.Sessions
            .AsNoTracking();

        if (movieId.HasValue)
        {
            query = query.Where(s =>
                s.MovieId == movieId.Value);
        }

        if (hallId.HasValue)
        {
            query = query.Where(s =>
                s.HallId == hallId.Value);
        }

        if (date.HasValue)
        {
            var startDate = date.Value.Date;
            var endDate = startDate.AddDays(1);

            query = query.Where(s =>
                s.StartTime >= startDate &&
                s.StartTime < endDate);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(s =>
                s.BasePrice >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(s =>
                s.BasePrice <= maxPrice.Value);
        }

        if (availableSeatsOnly == true)
        {
            var now = DateTime.UtcNow;

            query = query.Where(s =>
                _context.Seats.Any(seat =>
                    seat.HallId == s.HallId &&
                    !_context.Reservations.Any(r =>
                        r.SessionId == s.Id &&
                        r.SeatId == seat.Id &&
                        (
                            r.Status == "Confirmed" ||
                            (
                                r.Status == "Held" &&
                                r.ExpiresAt > now
                            )
                        )
                    )
                )
            );
        }

        return await query
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task AddAsync(Session session)
    {
        await _context.Sessions.AddAsync(session);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Session session)
    {
        _context.Sessions.Update(session);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Session session)
    {
        _context.Sessions.Remove(session);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasOverlappingSessionAsync(
        int hallId,
        DateTime startTime,
        DateTime endTime,
        int? excludeSessionId = null)
    {
        return await _context.Sessions
            .AnyAsync(s =>
                s.HallId == hallId &&
                (!excludeSessionId.HasValue ||
                 s.Id != excludeSessionId.Value) &&
                s.StartTime < endTime &&
                s.StartTime.AddMinutes(
                    s.Movie.DurationMinutes) > startTime);
    }

    public async Task<bool> HasReservationsAsync(int sessionId)
    {
        return await _context.Reservations
            .AnyAsync(r => r.SessionId == sessionId);
    }

    public async Task AddWithOverlapProtectionAsync(
        Session session,
        int durationMinutes)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        await AcquireHallLockAsync(session.HallId);

        var endTime = session.StartTime.AddMinutes(
            durationMinutes);

        var hasOverlap = await HasOverlappingSessionAsync(
            session.HallId,
            session.StartTime,
            endTime);

        if (hasOverlap)
        {
            throw new InvalidOperationException(
                "Another session is already scheduled in this hall.");
        }

        await _context.Sessions.AddAsync(session);
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();
    }

    
    public async Task UpdateWithOverlapProtectionAsync(
        Session session,
        int durationMinutes)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        var currentSession = await _context.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == session.Id);

        if (currentSession == null)
        {
            throw new KeyNotFoundException(
                "Session not found.");
        }

        var hallIds = new[]
            {
                currentSession.HallId,
                session.HallId
            }
            .Distinct()
            .OrderBy(id => id)
            .ToList();

        foreach (var hallId in hallIds)
        {
            await AcquireHallLockAsync(hallId);
        }

        var hasReservations = await HasReservationsAsync(
            session.Id);

        if (hasReservations)
        {
            throw new InvalidOperationException(
                "Cannot update a session that has reservations.");
        }

        var endTime = session.StartTime.AddMinutes(
            durationMinutes);

        var hasOverlap = await HasOverlappingSessionAsync(
            session.HallId,
            session.StartTime,
            endTime,
            session.Id);

        if (hasOverlap)
        {
            throw new InvalidOperationException(
                "Another session is already scheduled in this hall.");
        }

        _context.Sessions.Update(session);
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();
    }


    private async Task AcquireHallLockAsync(int hallId)
    {
        var connection = _context.Database.GetDbConnection();

        await using var command = connection.CreateCommand();

        command.Transaction =
            _context.Database.CurrentTransaction!
                .GetDbTransaction();

        command.CommandText = """
            DECLARE @result INT;

            EXEC @result = sp_getapplock
                @Resource = @resource,
                @LockMode = 'Exclusive',
                @LockOwner = 'Transaction',
                @LockTimeout = 10000;

            SELECT @result;
            """;

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@resource";
        parameter.Value = $"CinemaGo:Hall:{hallId}";
        command.Parameters.Add(parameter);

        var result = Convert.ToInt32(
            await command.ExecuteScalarAsync());

        if (result < 0)
        {
            throw new InvalidOperationException(
                "Could not acquire the hall scheduling lock.");
        }
    }
}
