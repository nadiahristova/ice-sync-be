using IceSync.Domain.Interfaces;
using IceSync.Domain.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Refit;
using System.Net;


namespace IceSync.Infrastructure.Policies;

public class RefitPolicyManager : PolicyManager<ApiException>, IRefitPolicyManager
{
    private readonly ILogger<RefitPolicyManager> _logger;

    private readonly HttpStatusCode[] _retryStatusCodesHttpResponse = {
        HttpStatusCode.RequestTimeout, HttpStatusCode.TooManyRequests, // 4xx
        HttpStatusCode.InternalServerError, HttpStatusCode.BadGateway, HttpStatusCode.ServiceUnavailable, HttpStatusCode.GatewayTimeout // 5xx
    };

    private readonly HttpStatusCode[] _breakCircuitStatusCodesHttpResponse = {
        HttpStatusCode.InternalServerError, HttpStatusCode.BadGateway, HttpStatusCode.ServiceUnavailable, HttpStatusCode.GatewayTimeout // 5xx
    };


    public RefitPolicyManager(
        IOptions<PollyPolicySettings> policySettings, 
        ILogger<RefitPolicyManager> logger)
        : base(policySettings?.Value ?? throw new ArgumentNullException(nameof(PollyPolicySettings)))

    {
        _logger = logger;
    }

    protected override IAsyncPolicy InitDefaultSimpleHttpRetry() => GetRetryPolicy(
            rr => _retryStatusCodesHttpResponse.Contains(rr.StatusCode),
            onRetry: (ex, retryCount, context) => {
                var ApiException = (ex as ApiException);
                _logger.LogWarning("Unable to send notification due to Http Status Code Error: {HttpStatusCode} and Firebase Error Code: {RefitHttpStatusCode} with message: {ErrorMessage} returned from the server. " +
                    "Retry count: {RetryCount}",
                ApiException.StatusCode, ApiException.StatusCode, ApiException.Message, retryCount);
            });

    protected override IAsyncPolicy InitDefaultSimpleWaitAndRetry()
        => GetWaitAndRetryPolicy(
            rr => _retryStatusCodesHttpResponse.Contains(rr.StatusCode),
            onRetry: (ex, timespan, retryCount, context) => {
                var ApiException = (ex as ApiException);
                _logger.LogWarning("Unable to send notification due to Http Status Code Error: {HttpStatusCode} and Firebase Error Code: {RefitHttpStatusCode} with message: {ErrorMessage} returned from the server. " +
                    "Retry count: {RetryCount}. Next try expected after: {NextTrySecs} secs.",
                    ApiException.StatusCode, ApiException.StatusCode, ApiException.Message, retryCount, timespan.Seconds);
            });

    protected override IAsyncPolicy InitDefaultTimeout()
        => GetTimeOutPolicy(onTimeOutAsync: (context, span, task) =>
        {
            _logger.LogWarning("Request timeout occurred after {NoResponseSpanSecs} secs.", span.Seconds);
            return Task.CompletedTask;
        });

    protected override IAsyncPolicy InitDefaultCircuitBreaker()
        => GetCircuitBreakerPolicy(
            rr => _breakCircuitStatusCodesHttpResponse.Contains(rr.StatusCode),
            onBreak: (ex, timespan, context) => {
                var ApiException = (ex as ApiException);
                _logger.LogWarning("Circuit breaker closed due to failed requests returned from the Firebase server. " +
                    "Http Status Code Error: {HttpStatusCode}; Firebase Error Code: {RefitHttpStatusCode} with message: {ErrorMessage}. Time until next test request secs: {NextRequestInSec}",
                    ApiException.StatusCode, ApiException.StatusCode, ApiException.Message, timespan.TotalSeconds);
            },
            onReset: ctx => {
                _logger.LogInformation("Circuit breaker opened at: {NextRequestInSec}", DateTime.UtcNow);
            },
            onHalfOpen: () => {
                _logger.LogInformation("Circuit breaker test request at: {NextRequestInSec}", DateTime.UtcNow);
            });

    protected override IAsyncPolicy InitDefaultAdvancedCircuitBreaker()
        => GetAdvancedCircuitBreakerPolicy(
                rr => _breakCircuitStatusCodesHttpResponse.Contains(rr.StatusCode),
                onBreak: (ex, state, timespan, context) => {
                    var ApiException = (ex as ApiException);
                    _logger.LogWarning("Circuit breaker closed due to failed requests returned from the Firebase server. " +
                        "Http Status Code Error: {HttpStatusCode}; Firebase Error Code: {RefitHttpStatusCode} with message: {ErrorMessage}. Time until next test request secs: {NextRequestInSec}",
                        ApiException.StatusCode, ApiException.StatusCode, ApiException.Message, timespan.TotalSeconds);
                },
                onReset: ctx => {
                    _logger.LogInformation("Circuit breaker opened at: {NextRequestInSec}", DateTime.UtcNow);
                },
                onHalfOpen: () => {
                    _logger.LogInformation("Circuit breaker test request at: {NextRequestInSec}", DateTime.UtcNow);
                });

    protected override IAsyncPolicy InitDefaultBulkHead()
        => GetBulkHeadPolicy();

    protected override IAsyncPolicy InitDefaultNoOps()
        => GetNoOpPolicy();
}
