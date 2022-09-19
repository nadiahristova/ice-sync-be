using AutoMapper;
using IceSync.Domain.Dtos;
using IceSync.Domain.Interfaces.HttpClients;
using IceSync.Domain.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static IceSync.Domain.Utils.Constants.TokenManagement;

namespace IceSync.Infrastructure.Services.TokenManagement;
public class UniversalLoaderTokenService : TokenService
{
    private readonly IMapper _mapper;
    private readonly IUniversalLoaderHttpClient _httpClient;
    private readonly IOptionsMonitor<UniversalLoaderSettings> _options;

    public UniversalLoaderTokenService(
        IMemoryCache memoryCache,
        IMapper mapper,
        IUniversalLoaderHttpClient httpClient,
        IOptionsMonitor<UniversalLoaderSettings> options,
        ILogger<UniversalLoaderTokenService> logger) : base(UniversalLoaderTokenKey, memoryCache, logger)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    protected override async Task<string> GetTokenFromApi(CancellationToken cancellationToken)
    {
        var loginData = _mapper.Map<UniversalLoaderLoginDto>(_options.CurrentValue);
        var tokenRaw = await _httpClient.GetToken(loginData, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Successful Universal Loader login.");

        return CleanToken(tokenRaw);
    }

    private string CleanToken(string token)
        => token.Replace("\"", string.Empty);
}