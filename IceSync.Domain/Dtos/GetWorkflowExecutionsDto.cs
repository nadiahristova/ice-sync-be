namespace IceSync.Domain.Dtos;

/// <summary>
/// Binding model of parameters needed for workflow executions data retrieval 
/// </summary>
/// <param name="WorkflowId">Id of the targeted workflow</param>
/// <param name="StartDate">Minimum date of workflow execution. UTC</param>
/// <param name="EndDate">Maximum date of workflow execution. UTC</param>
public record GetWorkflowExecutionsDto(int WorkflowId, DateTime? StartDate, DateTime? EndDate) : WorkflowIdWrapperDto(WorkflowId);
