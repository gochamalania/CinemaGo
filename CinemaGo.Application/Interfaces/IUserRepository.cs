using CinemaGo.Domain.Entities;

namespace CinemaGo.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);

    Task<User?> GetByIdAsync(int id);

    Task AddAsync(User user);

    Task UpdateAsync(User user);

    Task<bool> TrySetDateOfBirthAsync(
        int userId,
        DateTime dateOfBirth);

    Task<User?> UpdateProfileAsync(
        int userId,
        string firstName,
        string lastName,
        DateTime dateOfBirth);
}