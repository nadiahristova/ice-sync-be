using IceSync.Domain.Dtos;
using MediatR;

namespace IceSync.Infrastructure.Mediator.Requests;

public record GetAllWorkflowsRequest : IRequest<IEnumerable<WorkflowDto>>;
