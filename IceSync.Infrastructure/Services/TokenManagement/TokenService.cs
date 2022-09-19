using IceSync.Domain.Exceptions.Custom;
using IceSync.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;

namespace IceSync.Infrastructure.Services.TokenManagement;

public abstract class TokenService : ITokenService
{
    protected readonly string _tokenKey = null!;
    private readonly IMemoryCache _memoryCache;
    protected readonly ILogger<TokenService> _logger;

    public TokenService(string tokenKey, IMemoryCache memoryCache, ILogger<TokenService> logger)
    {
        _tokenKey = tokenKey;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves token from Memory Cache
    /// </summary>
    /// <returns>JWT token</returns>
    public async ValueTask<string> GetToken(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Requested token from memory cache.");
        if (!_memoryCache.TryGetValue(_tokenKey, out string token))
        {
            token = await GetTokenFromApi(cancellationToken).ConfigureAwait(false);
            var expirationDate = ExtractExpirationDateFormJWTToken(token);

            MemoryCacheEntryOptions entryOptions = new() { AbsoluteExpiration = expirationDate };

            _memoryCache.Set(_tokenKey, token, entryOptions);
            _logger.LogInformation("Token with key {TokenKey} and expiration date: {TokenExpirationDate}, saved in the memory cache.", _tokenKey, expirationDate);
        }

        return token;
    }

    protected abstract Task<string> GetTokenFromApi(CancellationToken cancellationToken);

    private DateTime ExtractExpirationDateFormJWTToken(string token)
    {
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var securityToken = tokenHandler.ReadToken(token);

            return securityToken.ValidTo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to extract expiration date from JWT token.");
            throw new InternalDomainException("Invalid authentication credential.");
        }
    }
}
