using CinemaGo.Domain.Entities;

namespace CinemaGo.Application.Interfaces;

public interface IHallRepository
{
    Task<List<Hall>> GetAllAsync();
    Task<Hall?> GetByIdAsync(int id);
    Task AddAsync(Hall hall);
    Task UpdateAsync(Hall hall);
    Task DeleteAsync(Hall hall);
    
    
    Task<bool> HasRelatedDataAsync(int hallId);

}