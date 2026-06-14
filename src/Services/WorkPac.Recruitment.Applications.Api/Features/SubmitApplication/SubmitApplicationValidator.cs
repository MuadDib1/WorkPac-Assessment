using FluentValidation;
using WorkPac.Recruitment.Contracts.ApiModels;

namespace WorkPac.Recruitment.Applications.Api.Features.SubmitApplication;

public class SubmitApplicationValidator : AbstractValidator<SubmitApplicationRequest>
{
    public SubmitApplicationValidator()
    {
        RuleFor(x => x.CandidateId).NotEmpty();
        RuleFor(x => x.CoverLetter).MaximumLength(5000);
        RuleForEach(x => x.Documents).ChildRules(doc =>
        {
            doc.RuleFor(d => d.FileName).NotEmpty().MaximumLength(255);
            doc.RuleFor(d => d.ContentType).NotEmpty().MaximumLength(100);
        });
    }
}
