namespace IceSync.Domain.Enums;

/// <summary>
/// Representation of Polly Policies.
/// </summary>
[Flags]
public enum PollyPolicy
{
    NoOps = 1, 
    FallBack = 2,
    Cache = 4,
    SimpleHttpRetry = 8,
    SimpleWaitAndRetry = 16,
    CircuitBreaker = 32,
    AdvancedCircuitBreaker = 64,
    BulkHead = 128,
    Timeout = 256
}
