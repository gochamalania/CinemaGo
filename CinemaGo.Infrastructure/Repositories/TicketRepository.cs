
using System.Data;
using CinemaGo.Application.Interfaces;
using CinemaGo.Domain.Entities;
using CinemaGo.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CinemaGo.Infrastructure.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly CinemaGoDbContext _context;

    public TicketRepository(CinemaGoDbContext context)
    {
        _context = context;
    }

    public async Task<Ticket?> GetByIdAsync(int id)
    {
        return await _context.Tickets
            .AsNoTracking()
            .Include(t => t.Reservation)
                .ThenInclude(r => r.Session)
            .Include(t => t.Reservation)
                .ThenInclude(r => r.Seat)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Ticket?> GetByReservationIdAsync(
        int reservationId)
    {
        return await _context.Tickets
            .AsNoTracking()
            .Include(t => t.Reservation)
            .FirstOrDefaultAsync(t =>
                t.ReservationId == reservationId);
    }

    public async Task<List<Ticket>> GetByUserIdAsync(
        int userId)
    {
        return await _context.Tickets
            .AsNoTracking()
            .Include(t => t.Reservation)
                .ThenInclude(r => r.Session)
            .Include(t => t.Reservation)
                .ThenInclude(r => r.Seat)
            .Where(t => t.Reservation.UserId == userId)
            .OrderByDescending(t => t.IssuedAt)
            .ToListAsync();
    }

    public async Task<bool> TicketNumberExistsAsync(
        string ticketNumber)
    {
        return await _context.Tickets
            .AnyAsync(t =>
                t.TicketNumber == ticketNumber);
    }

    public async Task AddAsync(Ticket ticket)
    {
        if (ticket.ReservationId <= 0)
            throw new ArgumentException(
                "Invalid reservation.");

        // Read seat identifiers before starting the transaction.
        var reservationInfo = await _context.Reservations
            .AsNoTracking()
            .Where(r => r.Id == ticket.ReservationId)
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

        // Always acquire locks in the same order.
        await AcquireLockAsync(
            $"CinemaGo:Seat:{reservationInfo.SessionId}:{reservationInfo.SeatId}",
            transaction);

        await AcquireLockAsync(
            $"CinemaGo:Ticket:Reservation:{ticket.ReservationId}",
            transaction);

        // Recheck after acquiring both locks.
        var ticketExists = await _context.Tickets
            .AnyAsync(t =>
                t.ReservationId == ticket.ReservationId);

        if (ticketExists)
            throw new InvalidOperationException(
                "Ticket already exists for this reservation.");

        var reservation = await _context.Reservations
            .AsNoTracking()
            .Include(r => r.Session)
            .FirstOrDefaultAsync(r =>
                r.Id == ticket.ReservationId);

        if (reservation == null)
            throw new KeyNotFoundException(
                "Reservation not found.");

        if (reservation.Status != "Confirmed")
            throw new InvalidOperationException(
                "Only confirmed reservations can receive tickets.");

        if (reservation.Session.StartTime <= DateTime.UtcNow)
            throw new InvalidOperationException(
                "Cannot issue tickets for a started session.");

        await _context.Tickets.AddAsync(ticket);
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();
    }

    private async Task AcquireLockAsync(
        string lockName,
        IDbContextTransaction transaction)
    {
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

        command.Parameters.Add(
            "@Resource",
            SqlDbType.NVarChar,
            255
        ).Value = lockName;

        var result = Convert.ToInt32(
            await command.ExecuteScalarAsync());

        if (result < 0)
            throw new InvalidOperationException(
                "Could not acquire database lock.");
    }
}
