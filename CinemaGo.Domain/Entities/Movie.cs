namespace CinemaGo.Domain.Entities;

public class Movie
{
    public int Id { get;  set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public DateTime ReleaseDate { get; set; }
}