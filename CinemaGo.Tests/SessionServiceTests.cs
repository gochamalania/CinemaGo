
using CinemaGo.Application.DTOs.Sessions;
using CinemaGo.Application.Interfaces;
using CinemaGo.Application.Services;
using CinemaGo.Domain.Entities;
using Moq;
using NUnit.Framework;

namespace CinemaGo.Tests;

[TestFixture]
public class SessionServiceTests
{
    private Mock<ISessionRepository> _sessions = null!;
    private Mock<IMovieRepository> _movies = null!;
    private Mock<IHallRepository> _halls = null!;
    private Mock<ISeatRepository> _seats = null!;
    private Mock<IReservationRepository> _reservations = null!;
    private SessionService _service = null!;

    [SetUp]
    public void Setup()
    {
        _sessions = new Mock<ISessionRepository>();
        _movies = new Mock<IMovieRepository>();
        _halls = new Mock<IHallRepository>();
        _seats = new Mock<ISeatRepository>();
        _reservations = new Mock<IReservationRepository>();

        _service = new SessionService(
            _sessions.Object,
            _movies.Object,
            _halls.Object,
            _seats.Object,
            _reservations.Object);

        _movies.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(new Movie
            {
                Id = 1,
                Title = "Test Movie",
                DurationMinutes = 120
            });

        _halls.Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(new Hall
            {
                Id = 1,
                Name = "Hall 1",
                Capacity = 100
            });
    }

    private static CreateSessionDto ValidSession()
    {
        return new CreateSessionDto
        {
            MovieId = 1,
            HallId = 1,
            StartTime = DateTime.UtcNow.AddDays(2),
            BasePrice = 20
        };
    }

    [Test]
    public void Create_PastDate_ThrowsException()
    {
        var dto = ValidSession();
        dto.StartTime = DateTime.UtcNow.AddDays(-1);

        Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.CreateAsync(dto));
    }

    [Test]
    public void Create_ZeroPrice_ThrowsException()
    {
        var dto = ValidSession();
        dto.BasePrice = 0;

        Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.CreateAsync(dto));
    }

    [Test]
    public void Create_NegativePrice_ThrowsException()
    {
        var dto = ValidSession();
        dto.BasePrice = -10;

        Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.CreateAsync(dto));
    }

    [Test]
    public void Create_MovieNotFound_ThrowsException()
    {
        var dto = ValidSession();
        dto.MovieId = 999;

        Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _service.CreateAsync(dto));
    }

    [Test]
    public void Create_HallNotFound_ThrowsException()
    {
        var dto = ValidSession();
        dto.HallId = 999;

        Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _service.CreateAsync(dto));
    }

    [Test]
    public void Create_OverlappingSession_ThrowsException()
    {
        var dto = ValidSession();

        _sessions.Setup(x => x.HasOverlappingSessionAsync(
                dto.HallId,
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<int?>()))
            .ReturnsAsync(true);

        Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.CreateAsync(dto));
    }
}
