using IceSync.Domain.Enums;
using Polly;
using Polly.CircuitBreaker;
using Polly.Timeout;
using Polly.Wrap;

namespace IceSync.Domain.Interfaces;
public interface IPolicyManager<TException> 
        where TException : Exception
{
    IAsyncPolicy<TDto> FallBackPolicy<TDto, KException>(TDto defaultValue)
        where TDto : class
        where KException : Exception;
    IAsyncPolicy GetAdvancedCircuitBreakerPolicy(Func<TException, bool> handleException, double? failureThreshold = null, int? minimumThroughput = null, TimeSpan? samplingDuration = null, TimeSpan? durationOfBreak = null, Action<Exception, CircuitState, TimeSpan, Context> onBreak = null, Action<Context> onReset = null, Action onHalfOpen = null);
    IAsyncPolicy GetBulkHeadPolicy(int? maxParallelization = null, int? maxQueuingActions = null, Func<Context, Task> onBulkheadRejectedAsync = null);
    IAsyncPolicy GetCircuitBreakerPolicy(Func<TException, bool> handleException, int? handledEventsAllowedBeforeBreaking = null, TimeSpan? durationOfBreak = null, Action<Exception, TimeSpan, Context> onBreak = null, Action<Context> onReset = null, Action onHalfOpen = null);
    IAsyncPolicy GetNoOpPolicy();
    IAsyncPolicy GetRetryPolicy(Func<TException, bool> handleException, int? retryCount = null, Action<Exception, int, Context> onRetry = null);
    IAsyncPolicy GetTimeOutPolicy(int? seconds = null, TimeoutStrategy? strategy = null, Func<Context, TimeSpan, Task, Task> onTimeOutAsync = null);
    IAsyncPolicy GetWaitAndRetryPolicy(Func<TException, bool> handleException, int? retryCount = null, Func<int, TimeSpan> sleepDurationProvider = null, Action<Exception, TimeSpan, int, Context> onRetry = null);
    IAsyncPolicy RetrieveDefaultPolicy(PollyPolicy policyType);
    AsyncPolicyWrap WrapDefaultPolicies(PollyPolicy policies);
    AsyncPolicyWrap WrapPolicies(params IAsyncPolicy[] policies);
}