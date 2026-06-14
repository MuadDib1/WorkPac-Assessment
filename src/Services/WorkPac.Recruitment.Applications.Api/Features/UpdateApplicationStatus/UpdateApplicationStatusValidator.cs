using FluentValidation;
using WorkPac.Recruitment.Contracts.ApiModels;

namespace WorkPac.Recruitment.Applications.Api.Features.UpdateApplicationStatus;

public class UpdateApplicationStatusValidator : AbstractValidator<UpdateApplicationStatusRequest>
{
    public UpdateApplicationStatusValidator()
    {
        RuleFor(x => x.NewStatus).IsInEnum();
        RuleFor(x => x.ChangedByUserId).NotEmpty();
        RuleFor(x => x.Reason).MaximumLength(1000);
    }
}
