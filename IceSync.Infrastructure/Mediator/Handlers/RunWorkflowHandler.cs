using IceSync.Domain.Interfaces;
using IceSync.Infrastructure.Mediator.Requests;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IceSync.Infrastructure.Mediator.Handlers;

public class RunWorkflowHandler : IRequestHandler<RunWorkflowRequest, Unit>
{
    private ILogger<RunWorkflowHandler> _logger;
    private IUniversalLoaderService _universalLoaderService;

    public RunWorkflowHandler(
            IUniversalLoaderService universalLoaderService,
            ILogger<RunWorkflowHandler> logger)
    {
        _logger = logger;
        _universalLoaderService = universalLoaderService;
    }
    public async Task<Unit> Handle(RunWorkflowRequest request, CancellationToken cancellationToken)
    {
        await _universalLoaderService.LaunchWorkflow(request.WorkflowId, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Workflow {WorkflowId} has been triggered.", request.WorkflowId);

        return Unit.Value;
    }
}
