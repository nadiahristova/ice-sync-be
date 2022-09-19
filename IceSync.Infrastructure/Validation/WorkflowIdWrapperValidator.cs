using FluentValidation;
using IceSync.Domain.Dtos;

namespace IceSync.Infrastructure.Validation;

public class WorkflowIdWrapperValidator : AbstractValidator<WorkflowIdWrapperDto>
{
    public WorkflowIdWrapperValidator()
    {
        RuleFor(w => w.WorkflowId)
            .GreaterThan(0);
    }
}
