using System.ComponentModel.DataAnnotations;

namespace CinemaGo.Application.DTOs.Movies;

public class UpdateMovieDto
{
    
    [Required]
    [MinLength(200)]
    public string Title { get; set; } =  String.Empty;
    
    [Required]
    public string Description { get; set; } = String.Empty;
    
    [Range (1, 1000)]
    public int DurationMinutes { get; set; }
    
    [Required]
    public DateTime ReleaseDate { get; set; }
    
    
}