using IceSync.Domain.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCrontab;

namespace IceSync.Infrastructure.BackgroundServices;
public abstract class ScheduledBackgroundService : BackgroundService
{
    protected readonly ILogger<ScheduledBackgroundService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly CrontabSchedule _schedule;

    private DateTime _nextRun;

    public ScheduledBackgroundService(
        IServiceScopeFactory serviceScopeFactory, 
        IOptions<ScheduleExecutionSettings> options,
        ILogger<ScheduledBackgroundService> logger)
    {
        var settings = options.Value ?? throw new ArgumentNullException(nameof(options));
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _schedule = CreateCronSchedule(settings.TriggerEvery);

        // comment to trigger the service on application run
        // _nextRun = _schedule.GetNextOccurrence(DateTime.Now);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Background service {this.GetType().Name} started at: {DateTime.UtcNow}");

        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(TimeUntilNextExecution(), cancellationToken);

            await Process(cancellationToken);

            _nextRun = _schedule.GetNextOccurrence(DateTime.Now.AddMilliseconds(1000));
            _logger.LogInformation($"Successful execution of {this.GetType().Name}. Next run will be expected at: {_nextRun}.");
        }

        await Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Background service {this.GetType().Name} stopped at: {DateTime.UtcNow}");

        await base.StopAsync(cancellationToken);
    }

    private CrontabSchedule CreateCronSchedule(string expression)
    {
        try
        {
            return CrontabSchedule.Parse(expression);
        }
        catch (CrontabException ex)
        {
            _logger.LogError(ex, "Invalid cron schedule expression format.");

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to create cron schedule.");

            throw;
        }
    }

    private int TimeUntilNextExecution() => Math.Max(0, (int)_nextRun.Subtract(DateTime.Now).TotalMilliseconds);

    private async Task Process(CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        await Process(scope.ServiceProvider, cancellationToken);
    }

    public abstract Task Process(IServiceProvider serviceProvider, CancellationToken cancellationToken);
}
