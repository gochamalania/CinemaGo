using CinemaGo.Application.Interfaces;
using CinemaGo.Domain.Entities;
using CinemaGo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaGo.Infrastructure.Repositories;

public class HallRepository : IHallRepository
{
    private readonly CinemaGoDbContext _context;

    public HallRepository(CinemaGoDbContext context)
    {
        _context = context;
    }

    public async Task<List<Hall>> GetAllAsync()
    {
        return await _context.Halls
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Hall?> GetByIdAsync(int id)
    {
        return await _context.Halls
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task AddAsync(Hall hall)
    {
        await _context.Halls.AddAsync(hall);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Hall hall)
    {
        _context.Halls.Update(hall);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Hall hall)
    {
        _context.Halls.Remove(hall);
        await _context.SaveChangesAsync();
    }
    
    
    public async Task<bool> HasRelatedDataAsync(int hallId)
    {
        var hasSeats = await _context.Seats
            .AnyAsync(s => s.HallId == hallId);

        var hasSessions = await _context.Sessions
            .AnyAsync(s => s.HallId == hallId);

        return hasSeats || hasSessions;
    }

    
}