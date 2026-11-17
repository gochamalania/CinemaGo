using CinemaGo.Application.DTOs.Seats;
using CinemaGo.Application.Interfaces;
using CinemaGo.Domain.Entities;

namespace CinemaGo.Application.Services;

public class SeatService : ISeatService
{
    private readonly ISeatRepository _seatRepository;
    private readonly IHallRepository _hallRepository;

    public SeatService(
        ISeatRepository seatRepository,
        IHallRepository hallRepository)
    {
        _seatRepository = seatRepository;
        _hallRepository = hallRepository;
    }

    
    public async Task<List<SeatResponseDto>> GetAllAsync()
    {
        var seats = await _seatRepository.GetAllAsync();

        return seats.Select(seat => new SeatResponseDto
        {
            Id = seat.Id,
            RowNumber = seat.RowNumber,
            SeatNumber = seat.SeatNumber,
            IsVip = seat.IsVip,
            HallId = seat.HallId
        }).ToList();
    }


    
    public async Task<SeatResponseDto?> GetByIdAsync(int id)
    {
        var seat = await _seatRepository.GetByIdAsync(id);

        if (seat == null)
        {
            return null;
        }

        return new SeatResponseDto
        {
            Id = seat.Id,
            RowNumber = seat.RowNumber,
            SeatNumber = seat.SeatNumber,
            IsVip = seat.IsVip,
            HallId = seat.HallId
        };
    }
    
    public async Task<List<SeatResponseDto>> GetByHallIdAsync(int hallId)
    {
        
        
        var hall = await _hallRepository.GetByIdAsync(hallId);

        if (hall == null)
        {
            throw new KeyNotFoundException("Hall not found.");
        }

        var seats = await _seatRepository.GetByHallIdAsync(hallId);

        return seats.Select(seat => new SeatResponseDto
        {
            Id = seat.Id,
            RowNumber = seat.RowNumber,
            SeatNumber = seat.SeatNumber,
            IsVip = seat.IsVip,
            HallId = seat.HallId
        }).ToList();
    }
    
    public async Task<SeatResponseDto> CreateAsync(CreateSeatDto request)
    {
        // 1. Check if hall exists
        var hall = await _hallRepository.GetByIdAsync(request.HallId);

        if (hall == null)
        {
            throw new KeyNotFoundException("Hall not found.");
        }

        // 2. Check hall capacity
        var seatCount = await _seatRepository
            .CountByHallIdAsync(request.HallId);

        if (seatCount >= hall.Capacity)
        {
            throw new InvalidOperationException(
                "Hall capacity has been reached.");
        }

        // 3. Check duplicate seat position
        var existingSeat = await _seatRepository.GetByPositionAsync(
            request.HallId,
            request.RowNumber,
            request.SeatNumber);

        if (existingSeat != null)
        {
            throw new InvalidOperationException(
                "Seat already exists in this position.");
        }

        // 4. Create seat entity
        var seat = new Seat
        {
            RowNumber = request.RowNumber,
            SeatNumber = request.SeatNumber,
            IsVip = request.IsVip,
            HallId = request.HallId
        };

        // 5. Save to database
        await _seatRepository.AddAsync(seat);

        // 6. Return response DTO
        return new SeatResponseDto
        {
            Id = seat.Id,
            RowNumber = seat.RowNumber,
            SeatNumber = seat.SeatNumber,
            IsVip = seat.IsVip,
            HallId = seat.HallId
        };
    }


    
    
    public async Task<bool> UpdateAsync(int id, UpdateSeatDto request)
    {
        // 1. Check if seat exists
        var seat = await _seatRepository.GetByIdAsync(id);

        if (seat == null)
        {
            return false;
        }

        // 2. Prevent moving seat to another hall
        if (seat.HallId != request.HallId)
        {
            throw new InvalidOperationException(
                "Moving a seat to another hall is not allowed.");
        }

        // 3. Check if any data is changing
        var hasChanges =
            seat.RowNumber != request.RowNumber ||
            seat.SeatNumber != request.SeatNumber ||
            seat.IsVip != request.IsVip;

        // 4. Prevent modifying seats with reservations
        if (hasChanges)
        {
            var hasReservations =
                await _seatRepository.HasReservationsAsync(id);

            if (hasReservations)
            {
                throw new InvalidOperationException(
                    "Seat cannot be modified because it has reservations.");
            }
        }

        // 5. Check duplicate position
        var existingSeat = await _seatRepository.GetByPositionAsync(
            request.HallId,
            request.RowNumber,
            request.SeatNumber);

        if (existingSeat != null && existingSeat.Id != id)
        {
            throw new InvalidOperationException(
                "Seat already exists in this position.");
        }

        // 6. Update seat
        seat.RowNumber = request.RowNumber;
        seat.SeatNumber = request.SeatNumber;
        seat.IsVip = request.IsVip;

        // 7. Save changes
        await _seatRepository.UpdateAsync(seat);

        return true;
    }



    public async Task<bool> DeleteAsync(int id)
    {
        // 1. Check if seat exists
        var seat = await _seatRepository.GetByIdAsync(id);

        if (seat == null)
        {
            return false;
        }

        // 2. Check if seat has reservations
        var hasReservations =
            await _seatRepository.HasReservationsAsync(id);

        if (hasReservations)
        {
            throw new InvalidOperationException(
                "Seat cannot be deleted because it has reservations.");
        }

        // 3. Delete seat
        await _seatRepository.DeleteAsync(seat);

        return true;
    }

}