using IceSync.Domain.Dtos;
using MediatR;

namespace IceSync.Infrastructure.Mediator.Requests;

public record GetWorkflowExecutionsRequest(int WorkflowId, DateTime? StartDate, DateTime? EndDate) : IRequest<IEnumerable<WorkflowExecutionDto>>;