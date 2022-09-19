using IceSync.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace IceSync.Domain.Entities;

public class Workflow : IEntity
{
    [Key]
    public int Id { get; set; }

    public int WorkflowId { get; set; }

    public string WorkflowName { get; set; } = null!;

    public bool IsActive { get; set; }

    public bool IsRunning { get; set; }

    [MaxLength(100)]
    public string MultiExecBehavior { get; set; } = null!;
}
