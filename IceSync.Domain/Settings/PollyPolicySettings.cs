using Polly.Timeout;
namespace IceSync.Domain.Settings;

public record PollyPolicySettings
{
    public RetryPolicySettings Retry { get; init; }

    public RetryPolicySettings WaitAndRetry { get; init; }

    public CircuitBreakerPolicySettings CircuitBreaker { get; init; }

    public AdvancedCircuitBreakerPolicySettings AdvancedCircuitBreaker { get; init; }

    public TimeoutPolicySettings Timeout { get; init; }

    public CachePolicySettings Cache { get; init; }

    public BulkHeadPolicySettings BulkHead { get; init; }
}

public record RetryPolicySettings
{
    public int RetryCount { get; init; }
}

public record CircuitBreakerPolicySettings
{
    public int FailedRequestsBeforeBreaking { get; init; }
    public int DurationOfBreakSecs { get; init; }
}

public record AdvancedCircuitBreakerPolicySettings
{
    public double FailureThreshold { get; init; }
    public int MinimumThroughput { get; init; }
    public int SamplingDurationSecs { get; init; }
    public int DurationOfBreakSecs { get; init; }
}

public record BulkHeadPolicySettings
{
    public int MaxParallelization { get; init; }
    public int MaxQueuingActions { get; init; }
}

public record TimeoutPolicySettings
{
    public int Seconds { get; init; }
    public TimeoutStrategy Strategy { get; init; }
}

public record CachePolicySettings
{
    public int Minutes { get; init; }
}
