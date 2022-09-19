namespace IceSync.Domain.Interfaces;

public interface ITokenService 
{
    /// <summary>
    /// Retrieves token either form Memory cache or an API
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>JWT</returns>
    ValueTask<string> GetToken(CancellationToken cancellationToken);
} 
