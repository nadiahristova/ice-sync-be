using IceSync.Domain.Entities;
using IceSync.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IceSync.Infrastructure.Repositories;
public class WorkflowReporitory : BaseDbRepository<Workflow, IceSyncContext>, IWorkflowReporitory
{
    public WorkflowReporitory(IceSyncContext currentContext) : base(currentContext)
    {
    }

    public async Task<IDictionary<int, int>> WorkflowIdToIdMapper(CancellationToken cancellationToken)
        => await _dbSet.AsNoTracking().ToDictionaryAsync(x => x.WorkflowId, x => x.Id, cancellationToken).ConfigureAwait(false);
}
