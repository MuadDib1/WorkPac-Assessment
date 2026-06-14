using FluentValidation;
using WorkPac.Recruitment.Contracts.ApiModels;

namespace WorkPac.Recruitment.Compliance.Api.Validators;

public class InitiateComplianceCheckValidator : AbstractValidator<InitiateComplianceCheckRequest>
{
    public InitiateComplianceCheckValidator()
    {
        RuleFor(x => x.CheckTypes)
            .NotNull().WithMessage("Check types are required.")
            .NotEmpty().WithMessage("At least one check type is required.");
        RuleForEach(x => x.CheckTypes)
            .NotEmpty().MaximumLength(50);
    }
}
