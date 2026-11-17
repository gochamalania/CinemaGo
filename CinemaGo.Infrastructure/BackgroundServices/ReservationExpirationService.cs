
using CinemaGo.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CinemaGo.Infrastructure.BackgroundServices;

public class ReservationExpirationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReservationExpirationService> _logger;

    public ReservationExpirationService(
        IServiceScopeFactory scopeFactory,
        ILogger<ReservationExpirationService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(
            TimeSpan.FromSeconds(30));

        try
        {
            // Check immediately when the service starts.
            await ExpireReservationsAsync(stoppingToken);

            // Check every 30 seconds.
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await ExpireReservationsAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            // Normal application shutdown.
        }
    }

    private async Task ExpireReservationsAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            await using var scope =
                _scopeFactory.CreateAsyncScope();

            var repository = scope.ServiceProvider
                .GetRequiredService<ReservationRepository>();

            var expiredCount =
                await repository.ExpireOldHoldsAsync(
                    cancellationToken);

            if (expiredCount > 0)
            {
                _logger.LogInformation(
                    "Successfully expired {Count} reservation holds.",
                    expiredCount);
            }
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while expiring reservation holds.");
        }
    }
}
