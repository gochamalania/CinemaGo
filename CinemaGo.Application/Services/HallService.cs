using CinemaGo.Application.DTOs.Halls;
using CinemaGo.Application.Interfaces;
using CinemaGo.Domain.Entities;

namespace CinemaGo.Application.Services;

public class HallService : IHallService
{
    private readonly IHallRepository _hallRepository;

    public HallService(IHallRepository hallRepository)
    {
        _hallRepository = hallRepository;
    }

    public async Task<List<HallResponseDto>> GetAllAsync()
    {
        var halls = await _hallRepository.GetAllAsync();

        return halls.Select(MapToDto).ToList();
    }

    public async Task<HallResponseDto?> GetByIdAsync(int id)
    {
        var hall = await _hallRepository.GetByIdAsync(id);

        if (hall == null)
            return null;

        return MapToDto(hall);
    }

    public async Task<HallResponseDto> CreateAsync(CreateHallDto request)
    {
        var hall = new Hall
        {
            Name = request.Name.Trim(),
            Capacity = request.Capacity
        };

        await _hallRepository.AddAsync(hall);

        return MapToDto(hall);
    }

    public async Task<bool> UpdateAsync(int id, UpdateHallDto request)
    {
        var hall = await _hallRepository.GetByIdAsync(id);

        if (hall == null)
            return false;

        hall.Name = request.Name.Trim();
        hall.Capacity = request.Capacity;

        await _hallRepository.UpdateAsync(hall);

        return true;
    }

    
    public async Task<bool> DeleteAsync(int id)
    {
        var hall = await _hallRepository.GetByIdAsync(id);

        if (hall == null)
            return false;

        var hasRelatedData =
            await _hallRepository.HasRelatedDataAsync(id);

        if (hasRelatedData)
        {
            throw new InvalidOperationException(
                "Hall cannot be deleted because it has seats or sessions.");
        }

        await _hallRepository.DeleteAsync(hall);

        return true;
    }


    private static HallResponseDto MapToDto(Hall hall)
    {
        return new HallResponseDto
        {
            Id = hall.Id,
            Name = hall.Name,
            Capacity = hall.Capacity
        };
    }
}