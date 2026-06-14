using WorkPac.Recruitment.Shared.Exceptions;
using WorkPac.Recruitment.Shared.Interfaces;

namespace WorkPac.Recruitment.Applications.Api.Features.WithdrawApplication;

public class WithdrawApplicationHandler
{
    private readonly IApplicationRepository _applicationRepo;

    public WithdrawApplicationHandler(IApplicationRepository applicationRepo)
    {
        _applicationRepo = applicationRepo;
    }

    public async Task HandleAsync(Guid id, CancellationToken ct)
    {
        var application = await _applicationRepo.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Application", id);

        application.Withdraw();
        await _applicationRepo.UpdateAsync(application, ct);
    }
}
