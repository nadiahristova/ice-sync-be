namespace IceSync.Domain.Dtos;

/// <summary>
/// Workflow Dto
/// </summary>
public class WorkflowDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool IsActive { get; set; }

    public bool IsRunning { get; set; }

    public string MultiExecBehavior { get; set; } = null!;
}
