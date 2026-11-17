
using CinemaGo.Application.Interfaces;
using CinemaGo.Domain.Entities;
using CinemaGo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CinemaGo.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly CinemaGoDbContext _context;

    public UserRepository(CinemaGoDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        await _context.SaveChangesAsync();
    }

    public async Task<bool> TrySetDateOfBirthAsync(
        int userId,
        DateTime dateOfBirth)
    {
        var affectedRows = await _context.Users
            .Where(u => u.Id == userId &&
                        u.DateOfBirth == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    u => u.DateOfBirth,
                    dateOfBirth.Date));

        return affectedRows == 1;
    }

    
    public async Task<User?> UpdateProfileAsync(
        int userId,
        string firstName,
        string lastName,
        DateTime dateOfBirth)
    {
        var normalizedDateOfBirth = dateOfBirth.Date;

        var affectedRows = await _context.Users
            .Where(u =>
                u.Id == userId &&
                (u.DateOfBirth == null ||
                 u.DateOfBirth == normalizedDateOfBirth))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(u => u.FirstName, firstName)
                .SetProperty(u => u.LastName, lastName)
                .SetProperty(
                    u => u.DateOfBirth,
                    normalizedDateOfBirth));

        if (affectedRows == 0)
        {
            var exists = await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.Id == userId);

            if (!exists)
                return null;

            throw new InvalidOperationException(
                "Date of birth cannot be changed once it has been set.");
        }

        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

}
