using IceSync.Domain.Dtos;
using IceSync.Domain.Enums;
using IceSync.Domain.Interfaces;
using IceSync.Domain.Interfaces.HttpClients;
using IceSync.Domain.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Refit;

namespace IceSync.Infrastructure.Services;

public class UniversalLoaderService : IUniversalLoaderService
{
    private readonly ITokenService _tokenService;
    private readonly IOptionsMonitor<UniversalLoaderSettings> _options;
    private readonly IRefitPolicyManager _refitPolicyManager;
    private readonly ILogger<UniversalLoaderService> _logger;

    private readonly PollyPolicy _appliedPolicies = PollyPolicy.AdvancedCircuitBreaker |
                                        PollyPolicy.SimpleWaitAndRetry | PollyPolicy.Timeout;

    public UniversalLoaderService(
        ITokenService tokenService,
        IOptionsMonitor<UniversalLoaderSettings> options,
        IRefitPolicyManager refitPolicyManager, 
        ILogger<UniversalLoaderService> logger)
    {
        _logger = logger;
        _options = options;
        _refitPolicyManager = refitPolicyManager;
        _tokenService = tokenService;
    }

    public async Task<IEnumerable<WorkflowDto>> GetWorkflows(CancellationToken cancellationToken)
    {
        var httpClient = await GetUniversalLoaderClient(cancellationToken).ConfigureAwait(false);

        return await _refitPolicyManager.WrapDefaultPolicies(_appliedPolicies)
                .ExecuteAsync(async _ => await httpClient.GetWorkflows(cancellationToken).ConfigureAwait(false),
                    cancellationToken);
    }

    public async Task<IEnumerable<WorkflowExecutionDto>> GetWorkflowsExecutions(int workflowId, DateTime? execStartFromUtcDateTime, DateTime? execStartToUtcDateTime, CancellationToken cancellationToken)
    {
        var httpClient = await GetUniversalLoaderClient(cancellationToken).ConfigureAwait(false);

        return await _refitPolicyManager.WrapDefaultPolicies(_appliedPolicies)
                .ExecuteAsync(async _ => await httpClient.GetWorkflowsExecutions(workflowId, execStartFromUtcDateTime, execStartToUtcDateTime, cancellationToken).ConfigureAwait(false),
                    cancellationToken);
    }

    public async Task LaunchWorkflow(int workflowId, CancellationToken cancellationToken)
    {
        var httpClient = await GetUniversalLoaderClient(cancellationToken).ConfigureAwait(false);

        await _refitPolicyManager.WrapDefaultPolicies(_appliedPolicies)
                .ExecuteAsync(async _ => await httpClient.LaunchWorkflow(workflowId, cancellationToken).ConfigureAwait(false),
                    cancellationToken);
    }

    private async Task<IUniversalLoaderHttpClient> GetUniversalLoaderClient(CancellationToken cancellationToken)
        => RestService.For<IUniversalLoaderHttpClient>(_options.CurrentValue.Url, new RefitSettings()
        {
            AuthorizationHeaderValueGetter = async () => await _tokenService.GetToken(cancellationToken).ConfigureAwait(false)
        });
}
