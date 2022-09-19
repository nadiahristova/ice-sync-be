using MediatR;

namespace IceSync.Infrastructure.Mediator.Requests;

public record RunWorkflowRequest(int WorkflowId) : IRequest<Unit>;
