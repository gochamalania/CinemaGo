
using System.Security;
using System.Security.Claims;
using CinemaGo.Application.DTOs.Reservations;
using CinemaGo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaGo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(
        IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    private int GetUserId()
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId) ||
            userId <= 0)
        {
            throw new UnauthorizedAccessException(
                "Invalid user token.");
        }

        return userId;
    }

    // POST: api/Reservations
    [HttpPost]
    [ProducesResponseType(
        typeof(ReservationResponseDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationResponseDto>> Create(
        [FromBody] CreateReservationDto dto)
    {
        try
        {
            var reservation = await _reservationService
                .CreateAsync(GetUserId(), dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = reservation.Id },
                reservation);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (SecurityException ex)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new { message = ex.Message });
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

    // GET: api/Reservations/my
    [HttpGet("my")]
    [ProducesResponseType(
        typeof(List<ReservationResponseDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<ReservationResponseDto>>>
        GetMyReservations()
    {
        try
        {
            var reservations = await _reservationService
                .GetMyReservationsAsync(GetUserId());

            return Ok(reservations);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    // GET: api/Reservations/{id}
    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(ReservationResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationResponseDto>> GetById(
        int id)
    {
        try
        {
            var reservation = await _reservationService
                .GetByIdAsync(id, GetUserId());

            if (reservation == null)
            {
                return NotFound(new
                {
                    message = "Reservation not found."
                });
            }

            return Ok(reservation);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    
    // POST: api/Reservations/{id}/confirm
    [HttpPost("{id:int}/confirm")]
    [ProducesResponseType(
        typeof(ReservationResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationResponseDto>> Confirm(
        int id)
    {
        try
        {
            var reservation = await _reservationService
                .ConfirmAsync(id, GetUserId());

            return Ok(reservation);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (SecurityException ex)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new { message = ex.Message });
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


    // POST: api/Reservations/{id}/cancel
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            await _reservationService.CancelAsync(
                id, GetUserId());

            return NoContent();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
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
}
