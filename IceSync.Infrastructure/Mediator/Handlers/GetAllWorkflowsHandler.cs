using IceSync.Domain.Dtos;
using IceSync.Domain.Interfaces;
using IceSync.Infrastructure.Mediator.Requests;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IceSync.Infrastructure.Mediator.Handlers;

public class GetAllWorkflowsHandler : IRequestHandler<GetAllWorkflowsRequest, IEnumerable<WorkflowDto>>
{
    private readonly ILogger<GetAllWorkflowsHandler> _logger;
    private readonly IUniversalLoaderService _universalLoaderService;

    public GetAllWorkflowsHandler(
        IUniversalLoaderService universalLoaderService,
        ILogger<GetAllWorkflowsHandler> logger)
    {
        _logger = logger;
        _universalLoaderService = universalLoaderService;
    }

    public async Task<IEnumerable<WorkflowDto>> Handle(GetAllWorkflowsRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Requested all worklogs form Universal loader service.");

        return await _universalLoaderService.GetWorkflows(cancellationToken).ConfigureAwait(false);
    }
}