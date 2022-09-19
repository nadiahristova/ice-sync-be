using IceSync.Domain.Dtos;

namespace IceSync.Domain.Interfaces;

public interface IUniversalLoaderService
{
    /// <summary>
    /// Gets wotkflow data
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>Collection of workflows</returns>
    Task<IEnumerable<WorkflowDto>> GetWorkflows(CancellationToken cancellationToken);

    /// <summary>
    /// Gets all executions for a given workflow and time frame
    /// </summary>
    /// <param name="workflowId"></param>
    /// <param name="execStartFromUtcDateTime"></param>
    /// <param name="execStartToUtcDateTime"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Workflow executions basic data</returns>
    Task<IEnumerable<WorkflowExecutionDto>> GetWorkflowsExecutions(int workflowId, DateTime? execStartFromUtcDateTime, DateTime? execStartToUtcDateTime, CancellationToken cancellationToken);

    /// <summary>
    /// Triggers execution of a given workflow
    /// </summary>
    /// <param name="workflowId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task LaunchWorkflow(int workflowId, CancellationToken cancellationToken);
}
