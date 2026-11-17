
using Microsoft.AspNetCore.Authorization;
using CinemaGo.Application.DTOs.Sessions;
using CinemaGo.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CinemaGo.API.Controllers;

[ApiController]
[Route("api/sessions")]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;

    public SessionsController(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    [HttpGet]
    public async Task<ActionResult<List<SessionResponseDto>>> GetAll()
    {
        var sessions = await _sessionService.GetAllAsync();

        return Ok(sessions);
    }

    [HttpGet("filter")]
    [AllowAnonymous]
    public async Task<ActionResult<List<SessionResponseDto>>> GetFiltered(
        [FromQuery] int? movieId,
        [FromQuery] int? hallId,
        [FromQuery] DateTime? date,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] bool? availableSeatsOnly)
    {
        try
        {
            var sessions = await _sessionService.GetFilteredAsync(
                movieId,
                hallId,
                date,
                minPrice,
                maxPrice,
                availableSeatsOnly);

            return Ok(sessions);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SessionResponseDto>> GetById(int id)
    {
        var session = await _sessionService.GetByIdAsync(id);

        if (session == null)
        {
            return NotFound(new
            {
                message = "Session not found."
            });
        }

        return Ok(session);
    }

    [HttpGet("{sessionId:int}/seats")]
    [ProducesResponseType(
        typeof(List<SessionSeatResponseDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<SessionSeatResponseDto>>> GetSessionSeats(
        int sessionId)
    {
        try
        {
            var seats = await _sessionService.GetSessionSeatsAsync(sessionId);

            return Ok(seats);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet("movie/{movieId:int}")]
    public async Task<ActionResult<List<SessionResponseDto>>> GetByMovieId(
        int movieId)
    {
        try
        {
            var sessions = await _sessionService.GetByMovieIdAsync(movieId);

            return Ok(sessions);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("hall/{hallId:int}")]
    public async Task<ActionResult<List<SessionResponseDto>>> GetByHallId(
        int hallId)
    {
        try
        {
            var sessions = await _sessionService.GetByHallIdAsync(hallId);

            return Ok(sessions);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SessionResponseDto>> Create(
        [FromBody] CreateSessionDto request)
    {
        try
        {
            var session = await _sessionService.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = session.Id },
                session);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
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
        [FromBody] UpdateSessionDto request)
    {
        try
        {
            var updated = await _sessionService.UpdateAsync(id, request);

            if (!updated)
            {
                return NotFound(new
                {
                    message = "Session not found."
                });
            }

            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
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
            var deleted = await _sessionService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Session not found."
                });
            }

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
