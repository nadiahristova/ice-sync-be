using IceSync.Domain.Entities;

namespace IceSync.Domain.Interfaces.Repositories;

public interface IWorkflowReporitory : IBaseRepository<Workflow>
{
    Task<IDictionary<int, int>> WorkflowIdToIdMapper(CancellationToken cancellationToken);
}
