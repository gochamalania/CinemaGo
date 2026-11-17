
using System.Security.Claims;
using System.Security;
using CinemaGo.Application.DTOs.Tickets;
using CinemaGo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaGo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    private int GetUserId()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out var id) || id <= 0)
        {
            throw new UnauthorizedAccessException(
                "Invalid user token.");
        }

        return id;
    }

    // POST: api/Tickets
    [HttpPost]
    [ProducesResponseType(
        typeof(TicketResponseDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TicketResponseDto>> Create(
        [FromBody] CreateTicketDto dto)
    {
        try
        {
            var ticket = await _ticketService.CreateAsync(
                GetUserId(), dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = ticket.Id },
                ticket);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
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

    // GET: api/Tickets/my
    [HttpGet("my")]
    [ProducesResponseType(
        typeof(List<TicketResponseDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<TicketResponseDto>>> GetMyTickets()
    {
        try
        {
            var tickets = await _ticketService.GetMyTicketsAsync(
                GetUserId());

            return Ok(tickets);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    // GET: api/Tickets/{id}
    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(TicketResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketResponseDto>> GetById(int id)
    {
        try
        {
            var ticket = await _ticketService.GetByIdAsync(
                id, GetUserId());

            if (ticket == null)
            {
                return NotFound(new
                {
                    message = "Ticket not found."
                });
            }

            return Ok(ticket);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}
