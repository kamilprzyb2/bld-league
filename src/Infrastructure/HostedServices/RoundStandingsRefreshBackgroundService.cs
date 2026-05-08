using BldLeague.Application.Commands.RoundStandings.RefreshAll;
using BldLeague.Infrastructure.Options;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BldLeague.Infrastructure.HostedServices;

public class RoundStandingsRefreshBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<RoundFinalizationOptions> options,
    ILogger<RoundStandingsRefreshBackgroundService> logger)
    : BackgroundService
{
    private readonly RoundFinalizationOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = ComputeDelayUntilNextFiring();
            logger.LogInformation(
                "RoundStandingsRefresh: next firing in {Delay} (at {Time} {TimeZone})",
                delay, $"{_options.Hour:D2}:{_options.Minute:D2}", _options.TimeZoneId);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            await FireAsync(stoppingToken);
        }
    }

    private TimeSpan ComputeDelayUntilNextFiring()
    {
        TimeZoneInfo tz;
        try
        {
            tz = TimeZoneInfo.FindSystemTimeZoneById(_options.TimeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            logger.LogWarning(
                "RoundStandingsRefresh: time zone '{TimeZoneId}' not found, falling back to UTC.",
                _options.TimeZoneId);
            tz = TimeZoneInfo.Utc;
        }

        var nowUtc = DateTime.UtcNow;
        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, tz);

        var todayFiring = new DateTime(nowLocal.Year, nowLocal.Month, nowLocal.Day, _options.Hour, _options.Minute, 0, DateTimeKind.Unspecified);
        var nextFiringLocal = nowLocal < todayFiring ? todayFiring : todayFiring.AddDays(1);
        var nextFiringUtc = TimeZoneInfo.ConvertTimeToUtc(nextFiringLocal, tz);

        var delay = nextFiringUtc - nowUtc;
        return delay < TimeSpan.Zero ? TimeSpan.Zero : delay;
    }

    private async Task FireAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("RoundStandingsRefresh: firing standings refresh.");
        try
        {
            using var scope = scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var result = await mediator.Send(new RefreshAllRoundStandingsRequest(), stoppingToken);
            if (result.Success)
                logger.LogInformation("RoundStandingsRefresh: standings refresh completed successfully.");
            else
                logger.LogError("RoundStandingsRefresh: standings refresh failed — {Message}", result.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "RoundStandingsRefresh: unhandled exception during standings refresh.");
        }
    }
}
