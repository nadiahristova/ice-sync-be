using IceSync.Domain.Dtos;
using IceSync.Domain.Interfaces;
using IceSync.Infrastructure.Mediator.Requests;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IceSync.Infrastructure.Mediator.Handlers;

public class GetWorkflowExecutionsHandler : IRequestHandler<GetWorkflowExecutionsRequest, IEnumerable<WorkflowExecutionDto>>
{
    private readonly ILogger<GetAllWorkflowsHandler> _logger;
    private readonly IUniversalLoaderService _universalLoaderService;

    public GetWorkflowExecutionsHandler(
        IUniversalLoaderService universalLoaderService,
        ILogger<GetAllWorkflowsHandler> logger)
    {
        _logger = logger;
        _universalLoaderService = universalLoaderService;
    }

    public async Task<IEnumerable<WorkflowExecutionDto>> Handle(GetWorkflowExecutionsRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Requested execution data for workflow with {WorkflowId} between dates {WorkflowStartExecutionDate} and {WorkflowEndExecutionDate}.", request.WorkflowId, request.StartDate, request.EndDate);

        return await _universalLoaderService.GetWorkflowsExecutions(request.WorkflowId, request.StartDate, request.EndDate, cancellationToken).ConfigureAwait(false);
    }
}