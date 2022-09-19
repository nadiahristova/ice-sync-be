using IceSync.Domain.Dtos;
using IceSync.Infrastructure.Mediator.Requests;
using IdempotentAPI.Filters;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
using static IceSync.Domain.Utils.Constants;

namespace IceSync.API.Controllers;

[ApiController]
[Route("[controller]")]
public class WorkflowsController : ControllerBase
{
    private readonly IMediator _mediator;

    public WorkflowsController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Retrieves all workflows available in Universal Loader API
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>Collection of workflows</returns>
    [HttpGet("get-all")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<IEnumerable<WorkflowDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllWorkflowsRequest(), cancellationToken).ConfigureAwait(false);

        return Ok(result);
    }

    /// <summary>
    /// Manually runs a workflow in Universal Loader API
    /// </summary>
    /// <param name="idWrapper">workflow Id</param>
    /// <param name="idempotencyKey">Idempotency key from 'IdempotencyKey' header</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("{WorkflowId}/run")]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesResponseType((int)HttpStatusCode.UnprocessableEntity)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [Consumes("application/json")]
    [Idempotent(Enabled = true, ExpireHours = 48)]
    public async Task<ActionResult> RunWorkflow([FromRoute] WorkflowIdWrapperDto idWrapper, [FromHeader(Name = IdempotencyKeyHeader)][Required] string idempotencyKey, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RunWorkflowRequest(idWrapper.WorkflowId), cancellationToken).ConfigureAwait(false);

        return NoContent();
    }

    /// <summary>
    /// Retrieves basic data for all workflow executions
    /// </summary>
    /// <param name="queryParams">inforrmation about the workflow and time frame</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Collection of basic workflow execution data</returns>
    [HttpGet("executions")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.UnprocessableEntity)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<IEnumerable<WorkflowExecutionDto>>> GetWorkflowExecutions([FromQuery] GetWorkflowExecutionsDto queryParams, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetWorkflowExecutionsRequest(queryParams.WorkflowId, queryParams.StartDate, queryParams.EndDate), cancellationToken).ConfigureAwait(false);

        return Ok(result);
    }
}