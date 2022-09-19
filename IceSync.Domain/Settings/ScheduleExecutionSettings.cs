namespace IceSync.Domain.Settings;

public record ScheduleExecutionSettings
{
    /// <summary>
    /// CRON expression
    /// </summary>
    public string TriggerEvery { get; init; } = null!;
}
