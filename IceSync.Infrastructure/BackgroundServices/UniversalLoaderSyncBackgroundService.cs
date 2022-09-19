using IceSync.Domain.Interfaces;
using IceSync.Domain.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IceSync.Infrastructure.BackgroundServices;

public class UniversalLoaderSyncBackgroundService : ScheduledBackgroundService
{
    public UniversalLoaderSyncBackgroundService(
        IServiceScopeFactory serviceScopeFactory, 
        IOptions<UniversalLoaderSyncSettings> options, 
        ILogger<ScheduledBackgroundService> logger) : base(serviceScopeFactory, options, logger)
    {
    }

    public override async Task Process(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var syncService = serviceProvider.GetRequiredService<IUniversalLoaderSyncService>();

        try
        {
            await syncService.SyncData(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Synchronise worklog data with the Universal Loader.");
            throw;
        }
    }
}
