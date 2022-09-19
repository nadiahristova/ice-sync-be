namespace IceSync.Domain.Interfaces;

public interface IUniversalLoaderSyncService
{
    /// <summary>
    /// Synchronises workflow data form Universal Loader API
    /// </summary>
    /// <param name="cancellationToken"></param>
    Task SyncData(CancellationToken cancellationToken);
}
