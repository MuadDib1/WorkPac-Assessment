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
            doc.RuleFor(d => d.Base64Content)
                .NotEmpty()
                .Must(BeValidBase64).WithMessage("Document content is not valid base64.")
                .Must(NotExceedSize).WithMessage("Document content exceeds 10MB limit.");
        });
        When(x => x.Answers is not null, () =>
        {
            RuleForEach(x => x.Answers!).ChildRules(kvp =>
            {
                kvp.RuleFor(k => k.Key).NotEmpty().MaximumLength(100);
                kvp.RuleFor(k => k.Value).MaximumLength(2000);
            });
        });
    }

    private static bool BeValidBase64(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        try { _ = Convert.FromBase64String(value); return true; }
        catch { return false; }
    }

    private static bool NotExceedSize(string value)
    {
        if (string.IsNullOrEmpty(value)) return true;
        try
        {
            var bytes = Convert.FromBase64String(value);
            return bytes.Length <= 10 * 1024 * 1024;
        }
        catch { return false; }
    }
}
