namespace IceSync.Domain.Dtos;

/// <summary>
/// Basic Workflow Execution Dto
/// </summary>
public class WorkflowExecutionDto
{
    public int Id { get; set; }
    public int WorkflowId { get; set; }
    public string? WorkflowName { get; set; }
    public DateTime startDateTime { get; set; }
    public DateTime? stopDateTime { get; set; }
    public string ExecutionStatus { get; set; } = null!;
    public string WorkflowExecutionType { get; set; } = null!;
    public string? ExecutionUserName { get; set; }
    public int? RetryNumber { get; set; }
}

