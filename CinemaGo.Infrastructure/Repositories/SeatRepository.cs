using CinemaGo.Application.Interfaces;
using CinemaGo.Domain.Entities;
using CinemaGo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaGo.Infrastructure.Repositories;

public class SeatRepository : ISeatRepository
{
    private readonly CinemaGoDbContext _context;
    
    public SeatRepository(CinemaGoDbContext context)
    {
        _context = context;
    }

    public async Task<List<Seat>> GetAllAsync()
    {
        return await _context.Seats
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Seat?> GetByIdAsync(int id)
    {
        return await _context.Seats
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<Seat>> GetByHallIdAsync(int hallId)
    {
        return await _context.Seats
            .AsNoTracking()
            .Where(s => s.HallId == hallId)
            .OrderBy(s => s.RowNumber)
            .ThenBy(s => s.SeatNumber)
            .ToListAsync();
    }

    public async Task<Seat?> GetByPositionAsync(int hallId, int rowNumber, int seatNumber)
    {
        return await _context.Seats
            .AsNoTracking()
            .FirstOrDefaultAsync(s =>
                s.HallId == hallId &&
                s.RowNumber == rowNumber &&
                s.SeatNumber == seatNumber);
    }

    public async Task<int> CountByHallIdAsync(int hallId)
    {
        return await _context.Seats
            .CountAsync(s => s.HallId == hallId);
    }

    public async Task AddAsync(Seat seat)
    {
        await _context.Seats.AddAsync(seat);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Seat seat)
    {
        _context.Seats.Update(seat);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Seat seat)
    {
        _context.Seats.Remove(seat);
        await _context.SaveChangesAsync();
    }
    
    
    public async Task<bool> HasReservationsAsync(int seatId)
    {
        return await _context.Reservations
            .AnyAsync(r => r.SeatId == seatId);
    }

    
}