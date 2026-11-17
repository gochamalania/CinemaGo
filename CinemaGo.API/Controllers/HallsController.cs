using CinemaGo.Application.DTOs.Halls;
using CinemaGo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaGo.API.Controllers;

[ApiController]
[Route("api/halls")]
public class HallsController : ControllerBase
{
    private readonly IHallService _hallService;

    public HallsController(IHallService hallService)
    {
        _hallService = hallService;
    }

    // GET: api/halls
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<HallResponseDto>>> GetAll()
    {
        var halls = await _hallService.GetAllAsync();

        return Ok(halls);
    }

    // GET: api/halls/1
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<HallResponseDto>> GetById(int id)
    {
        var hall = await _hallService.GetByIdAsync(id);

        if (hall == null)
            return NotFound();

        return Ok(hall);
    }

    // POST: api/halls
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<HallResponseDto>> Create(
        [FromBody] CreateHallDto request)
    {
        var hall = await _hallService.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = hall.Id },
            hall);
    }

    // PUT: api/halls/1
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateHallDto request)
    {
        var updated = await _hallService.UpdateAsync(id, request);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    // DELETE: api/halls/1
    
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _hallService.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

}