using Microsoft.AspNetCore.Authorization;

using CinemaGo.Application.DTOs.Seats;
using CinemaGo.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CinemaGo.API.Controllers;

[ApiController]
[Route("api/seats")]
public class SeatsController : ControllerBase
{
    private readonly ISeatService _seatService;

    public SeatsController(ISeatService seatService)
    {
        _seatService = seatService;
    }

    [HttpGet]
    public async Task<ActionResult<List<SeatResponseDto>>> GetAll()
    {
        var seats = await _seatService.GetAllAsync();

        return Ok(seats);
    }
    
    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<SeatResponseDto>> GetById(int id)
    {
        var seat = await _seatService.GetByIdAsync(id);

        if (seat == null)
        {
            return NotFound();
        }

        return Ok(seat);
    }
    
    
    
    [HttpGet("hall/{hallId:int}")]
    public async Task<ActionResult<List<SeatResponseDto>>> GetByHallId(int hallId)
    {
        try
        {
            var seats = await _seatService.GetByHallIdAsync(hallId);

            return Ok(seats);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
    
    
    
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SeatResponseDto>> Create(
        [FromBody] CreateSeatDto request)
    {
        try
        {
            var seat = await _seatService.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = seat.Id },
                seat);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateSeatDto request)
    {
        try
        {
            var updated = await _seatService.UpdateAsync(id, request);

            if (!updated)
            {
                return NotFound(new { message = "Seat not found." });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
    
    
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _seatService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new { message = "Seat not found." });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }


}