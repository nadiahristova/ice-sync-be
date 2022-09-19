using FluentValidation;
using IceSync.Domain.Dtos;

namespace IceSync.Infrastructure.Validation;

public class GetWorkflowExecutionsValidator : AbstractValidator<GetWorkflowExecutionsDto>
{
    private readonly DateTime _minDate = DateTime.Now.AddMonths(-5);
    public GetWorkflowExecutionsValidator()
    {
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate).When(x => x.StartDate.HasValue)
            .GreaterThanOrEqualTo(_ => _minDate).When(x => x.EndDate.HasValue);

        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(_ => _minDate)
            .When(x => x.StartDate.HasValue);

        Include(new WorkflowIdWrapperValidator());
    }
}
