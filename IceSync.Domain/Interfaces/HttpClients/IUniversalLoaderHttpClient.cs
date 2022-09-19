using IceSync.Domain.Dtos;
using Refit;

namespace IceSync.Domain.Interfaces.HttpClients;

public interface IUniversalLoaderHttpClient
{
    /// <summary>
    /// Authenticates against Universal Loader
    /// </summary>
    /// <param name="loginData"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>JWT</returns>
    [Post("/authenticate")]
    Task<string> GetToken([Body] UniversalLoaderLoginDto loginData, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all workflows
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>All workflows</returns>
    [Get("/Workflows")]
    [Headers("Authorization: Bearer")]
    Task<IEnumerable<WorkflowDto>> GetWorkflows(CancellationToken cancellationToken);

    /// <summary>
    /// Get executions basic data for given workflow
    /// </summary>
    /// <param name="workflowId"></param>
    /// <param name="execStartFromUtcDateTime"></param>
    /// <param name="execStartToUtcDateTime"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Get("/Workflows/executions")]
    [Headers("Authorization: Bearer")]
    Task<IEnumerable<WorkflowExecutionDto>> GetWorkflowsExecutions(int workflowId, DateTime? execStartFromUtcDateTime, DateTime? execStartToUtcDateTime, CancellationToken cancellationToken);

    /// <summary>
    /// Triggers workflow execution
    /// </summary>
    /// <param name="workflowId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Post("/Workflows/{workflowId}/run")]
    [Headers("Authorization: Bearer")]
    Task LaunchWorkflow(int workflowId, CancellationToken cancellationToken);
}
