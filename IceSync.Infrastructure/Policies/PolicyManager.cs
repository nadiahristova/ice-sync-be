using IceSync.Domain.Enums;
using IceSync.Domain.Interfaces;
using IceSync.Domain.Settings;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using Polly.Wrap;

namespace IceSync.Infrastructure.Policies;

public abstract class PolicyManager<TException> : IPolicyManager<TException>
        where TException : Exception
{
    protected readonly PollyPolicySettings _policySettings;

    protected readonly IAsyncPolicy _simpleHttpRetry;
    protected readonly IAsyncPolicy _simpleWaitAndRetry;
    protected readonly IAsyncPolicy _timeout;
    protected readonly IAsyncPolicy _circuitBreaker;
    protected readonly IAsyncPolicy _advancedCircuitBreaker;
    protected readonly IAsyncPolicy _bulkHead;
    protected readonly IAsyncPolicy _noOps;

    public IEnumerable<PollyPolicy> PollyPolicyWrapOrder => _pollyPolicyWrapOrder.Value;

    protected readonly Lazy<IEnumerable<PollyPolicy>> _pollyPolicyWrapOrder
        = new Lazy<IEnumerable<PollyPolicy>>(() => ((PollyPolicy[])Enum.GetValues(typeof(PollyPolicy)))
            .Where(x => x != PollyPolicy.NoOps));


    public PolicyManager(PollyPolicySettings policySettings)
    {
        _policySettings = policySettings;

        _simpleHttpRetry = InitDefaultSimpleHttpRetry();
        _simpleWaitAndRetry = InitDefaultSimpleWaitAndRetry();
        _timeout = InitDefaultTimeout();
        _circuitBreaker = InitDefaultCircuitBreaker();
        _advancedCircuitBreaker = InitDefaultAdvancedCircuitBreaker();
        _bulkHead = InitDefaultBulkHead();
        _noOps = InitDefaultNoOps();
    }

    public IAsyncPolicy RetrieveDefaultPolicy(PollyPolicy policyType)
        => policyType switch
        {
            PollyPolicy.SimpleHttpRetry => _simpleHttpRetry,
            PollyPolicy.SimpleWaitAndRetry => _simpleWaitAndRetry,
            PollyPolicy.NoOps => _noOps,
            PollyPolicy.CircuitBreaker => _circuitBreaker,
            PollyPolicy.AdvancedCircuitBreaker => _advancedCircuitBreaker,
            PollyPolicy.Timeout => _timeout,
            PollyPolicy.BulkHead => _bulkHead,
            _ => throw new NotImplementedException("Polly policy not implemented.")
        };

    /// <summary>
    ///     Wraps Polly policies.
    /// </summary>
    /// <param name="policies">Polly policies that wrap one another.</param>
    /// <remarks>
    ///     The order of the policies matter. First specified policy is the outermost, last policy is the inner most
    ///     policy in the wrapping.
    /// </remarks>
    /// <returns>A combined policy strategy, built of predefined policies.</returns>
    public AsyncPolicyWrap WrapPolicies(params IAsyncPolicy[] policies)
    {
        return Policy.WrapAsync(policies);
    }

    /// <summary>
    ///     Returns wrapped default Polly policies.
    /// </summary>
    /// <param name="policies">Polly policy enums.</param>
    /// <returns>A combined policy strategy, built of predefined policies.</returns>
    public AsyncPolicyWrap WrapDefaultPolicies(PollyPolicy policies)
    {
        return Policy.WrapAsync(GetDefaultPolicies(policies));
    }

    /// <summary>
    ///     Gets Retry policy.
    /// </summary>
    /// <param name="handleException">Condition for failed request.</param>
    /// <param name="retryCount">Number of retries in case of continuously failing requests.</param>
    /// <param name="onRetry">Actions performed on failed request.</param>
    /// <returns></returns>
    public IAsyncPolicy GetRetryPolicy(
        Func<TException, bool> handleException,
        int? retryCount = null,
        Action<Exception, int, Context>? onRetry = null)
    {
        retryCount ??= _policySettings.Retry.RetryCount;
        onRetry ??= (exception, retry, context) => { };

        var retryPolicy = Policy
            .Handle(handleException)
            .RetryAsync(retryCount.Value, onRetry);

        return retryPolicy;
    }

    /// <summary>
    ///     Gets Wait And Retry policy.
    /// </summary>
    /// <param name="handleException">Condition for failed request.</param>
    /// <param name="retryCount">Number of retries in case of continuously failing requests.</param>
    /// <param name="sleepDurationProvider">Function that determines the time span between failing requests.</param>
    /// <param name="onRetry">Actions performed on failed request.</param>
    /// <returns></returns>
    public IAsyncPolicy GetWaitAndRetryPolicy(
        Func<TException, bool> handleException,
        int? retryCount = null,
        Func<int, TimeSpan>? sleepDurationProvider = null,
        Action<Exception, TimeSpan, int, Context>? onRetry = null)
    {
        retryCount ??= _policySettings.WaitAndRetry.RetryCount;
        sleepDurationProvider ??= retryAttempts => TimeSpan.FromSeconds(Math.Pow(2, retryAttempts));
        onRetry ??= (exception, timespan, retryCount, context) => { };

        var waitAndRetryPolicy = Policy
            .Handle(handleException)
            .WaitAndRetryAsync(retryCount.Value, sleepDurationProvider, onRetry);

        return waitAndRetryPolicy;
    }

    /// <summary>
    ///     Gets NoOp policy. A policy which will cause delegates passed for execution to be executed 'as is'.
    /// </summary>
    /// <returns></returns>
    public IAsyncPolicy GetNoOpPolicy()
        => Policy.NoOpAsync();

    /// <summary>
    ///     Gets Circuit Breaker policy.
    /// </summary>
    /// <param name="handleException">Condition for failed request.</param>
    /// <param name="handledEventsAllowedBeforeBreaking">Number of failed requests before the circuit is open.</param>
    /// <param name="durationOfBreak">Time span during which requests are no longer possible. The circuit is open.</param>
    /// <param name="onBreak">Action executed when number of failed requests reach the maximum. The circuit is open.</param>
    /// <param name="onReset">
    ///     Action executed after duration of break has timed out and the first request after the period is
    ///     successful. The circuit has closed.
    /// </param>
    /// <param name="onHalfOpen">Action executed after duration of break has passed. The circuit is still open.</param>
    /// <returns></returns>
    public IAsyncPolicy GetCircuitBreakerPolicy(
        Func<TException, bool> handleException,
        int? handledEventsAllowedBeforeBreaking = null,
        TimeSpan? durationOfBreak = null,
        Action<Exception, TimeSpan, Context>? onBreak = null,
        Action<Context>? onReset = null,
        Action? onHalfOpen = null)
    {
        handledEventsAllowedBeforeBreaking ??= _policySettings.CircuitBreaker.FailedRequestsBeforeBreaking;
        durationOfBreak ??= TimeSpan.FromSeconds(_policySettings.CircuitBreaker.DurationOfBreakSecs);
        onBreak ??= (exception, timespan, context) => { };
        onReset ??= ctx => { };
        onHalfOpen ??= () => { };

        var circuitBreakerPolicy = Policy
            .Handle(handleException)
            .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking.Value, durationOfBreak.Value, onBreak, onReset,
                onHalfOpen);

        return circuitBreakerPolicy;
    }

    /// <summary>
    ///     Gets Advanced Circuit Breaker policy
    /// </summary>
    /// <param name="handleException">Condition for failed request.</param>
    /// <param name="failureThreshold">Percent of failed requests during specified time period.</param>
    /// <param name="minimumThroughput">Minimum number of failed requests during specified time period.</param>
    /// <param name="samplingDuration">The specified time period.</param>
    /// <param name="durationOfBreak">Time span during which requests are no longer possible. The circuit is open.</param>
    /// <param name="onBreak">Action executed when number of failed requests reach the maximum. The circuit has open.</param>
    /// <param name="onReset">
    ///     Action executed after duration of break has timed out and the first request after the period is
    ///     successful. The circuit has closed.
    /// </param>
    /// <param name="onHalfOpen">Action executed after duration of break has passed. The circuit is still open.</param>
    /// <returns></returns>
    public IAsyncPolicy GetAdvancedCircuitBreakerPolicy(
        Func<TException, bool> handleException,
        double? failureThreshold = null,
        int? minimumThroughput = null,
        TimeSpan? samplingDuration = null,
        TimeSpan? durationOfBreak = null,
        Action<Exception, CircuitState, TimeSpan, Context>? onBreak = null,
        Action<Context>? onReset = null,
        Action? onHalfOpen = null)
    {
        failureThreshold ??= _policySettings.AdvancedCircuitBreaker.FailureThreshold;
        minimumThroughput ??= _policySettings.AdvancedCircuitBreaker.MinimumThroughput;
        samplingDuration ??= TimeSpan.FromSeconds(_policySettings.AdvancedCircuitBreaker.SamplingDurationSecs);
        durationOfBreak ??= TimeSpan.FromSeconds(_policySettings.AdvancedCircuitBreaker.DurationOfBreakSecs);
        onBreak ??= (exception, state, timespan, context) => { };
        onReset ??= ctx => { };
        onHalfOpen ??= () => { };

        var advancedCircuitBreakerPolicy = Policy
            .Handle(handleException)
            .AdvancedCircuitBreakerAsync(failureThreshold.Value, samplingDuration.Value, minimumThroughput.Value,
                durationOfBreak.Value, onBreak, onReset, onHalfOpen);

        return advancedCircuitBreakerPolicy;
    }

    /// <summary>
    ///     Gets FallBack policy
    /// </summary>
    /// <typeparam name="TDto">Returned type.</typeparam>
    /// <typeparam name="KException">Occurring exception.</typeparam>
    /// <param name="defaultValue">Returned default value.</param>
    /// <returns></returns>
    public IAsyncPolicy<TDto> FallBackPolicy<TDto, KException>(TDto defaultValue)
        where TDto : class
        where KException : Exception
    {
        var fallBackPolicy = Policy
            .Handle<KException>()
            .OrResult(default(TDto))
            .FallbackAsync(defaultValue);

        return fallBackPolicy;
    }

    /// <summary>
    ///     Gets Bulk Head policy
    /// </summary>
    /// <param name="maxParallelization">Maximum number of send requests.</param>
    /// <param name="maxQueuingActions">Maximum size of the queue.</param>
    /// <param name="onBulkheadRejectedAsync">Action executed when there is no available space in the request queue.</param>
    /// <returns></returns>
    public IAsyncPolicy GetBulkHeadPolicy(
        int? maxParallelization = null,
        int? maxQueuingActions = null,
        Func<Context, Task> onBulkheadRejectedAsync = null)
    {
        maxParallelization ??= _policySettings.BulkHead.MaxParallelization;
        maxQueuingActions ??= _policySettings.BulkHead.MaxQueuingActions;
        onBulkheadRejectedAsync ??= context => Task.CompletedTask;

        var bulkHeadPolicy = Policy
            .BulkheadAsync(maxParallelization.Value, maxQueuingActions.Value, onBulkheadRejectedAsync);

        return bulkHeadPolicy;
    }

    /// <summary>
    ///     Gets Timeout policy.
    /// </summary>
    /// <param name="seconds">Number of seconds before timeout occur.</param>
    /// <param name="strategy">Time out strategy.</param>
    /// <param name="onTimeOutAsync">Action executed when timeout occur.</param>
    /// <returns></returns>
    public IAsyncPolicy GetTimeOutPolicy(
        int? seconds = null,
        TimeoutStrategy? strategy = null,
        Func<Context, TimeSpan, Task, Task> onTimeOutAsync = null)
    {
        seconds ??= _policySettings.Timeout.Seconds;
        strategy ??= _policySettings.Timeout.Strategy;
        onTimeOutAsync ??= (context, span, task) => Task.CompletedTask;

        var timeoutPolicy = Policy
            .TimeoutAsync(seconds.Value, strategy.Value, onTimeOutAsync);

        return timeoutPolicy;
    }

    protected abstract IAsyncPolicy InitDefaultSimpleHttpRetry();
    protected abstract IAsyncPolicy InitDefaultSimpleWaitAndRetry();
    protected abstract IAsyncPolicy InitDefaultTimeout();
    protected abstract IAsyncPolicy InitDefaultCircuitBreaker();
    protected abstract IAsyncPolicy InitDefaultAdvancedCircuitBreaker();
    protected abstract IAsyncPolicy InitDefaultBulkHead();
    protected abstract IAsyncPolicy InitDefaultNoOps();

    private IAsyncPolicy[] GetDefaultPolicies(PollyPolicy policyTypes)
    {
        if (policyTypes.HasFlag(PollyPolicy.NoOps))
        {
            throw new ArgumentException("No Op policy should not be used in wrapping.");
        }

        // specify the correct order of the policies
        var policyEnums = _pollyPolicyWrapOrder.Value
            .Where(pt => policyTypes.HasFlag(pt))
            .OrderBy(x => x)
            .ToList();

        List<IAsyncPolicy> policies = new();
        policyEnums.ForEach(pe => policies.Add(RetrieveDefaultPolicy(pe)));

        return policies.ToArray();
    }
}