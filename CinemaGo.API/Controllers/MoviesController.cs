
using CinemaGo.Application.DTOs.Movies;
using CinemaGo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaGo.API.Controllers;

[ApiController]
[Route("api/movies")]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<MovieResponseDto>>> GetAll()
    {
        var movies = await _movieService.GetAllAsync();

        return Ok(movies);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<MovieResponseDto>> GetById(int id)
    {
        var movie = await _movieService.GetByIdAsync(id);

        if (movie == null)
        {
            return NotFound(new
            {
                message = "Movie not found."
            });
        }

        return Ok(movie);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(
        typeof(MovieResponseDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MovieResponseDto>> Create(
        [FromBody] CreateMovieDto request)
    {
        try
        {
            var movie = await _movieService.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = movie.Id },
                movie);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateMovieDto request)
    {
        try
        {
            var updated = await _movieService.UpdateAsync(id, request);

            if (!updated)
            {
                return NotFound(new
                {
                    message = "Movie not found."
                });
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _movieService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Movie not found."
            });
        }

        return NoContent();
    }
}
