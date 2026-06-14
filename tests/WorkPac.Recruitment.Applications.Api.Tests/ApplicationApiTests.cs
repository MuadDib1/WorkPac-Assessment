using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using WorkPac.Recruitment.Contracts.ApiModels;
using WorkPac.Recruitment.Shared.Domain;
using WorkPac.Recruitment.Shared.Enums;
using WorkPac.Recruitment.Shared.ValueObjects;

namespace WorkPac.Recruitment.Applications.Api.Tests;

public class ApplicationApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public ApplicationApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        content.Should().ContainKey("status");
        content!["status"].ToString().Should().Be("Healthy");
    }

    [Fact]
    public async Task SubmitApplication_WithValidData_ReturnsCreated()
    {
        var jobId = await SeedJobPostingAsync();
        var candidateId = await SeedCandidateAsync();

        var request = new SubmitApplicationRequest(
            candidateId,
            "I am a experienced diesel fitter with 5 years in open-cut mining.",
            null,
            null);

        var response = await _client.PostAsJsonAsync($"/v1/jobs/{jobId}/applications", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var app = await response.Content.ReadFromJsonAsync<ApplicationResponse>();
        app.Should().NotBeNull();
        app!.JobPostingId.Should().Be(jobId);
        app.CandidateId.Should().Be(candidateId);
        app.Status.Should().Be(ApplicationStatus.Submitted);
    }

    [Fact]
    public async Task SubmitApplication_Duplicate_ReturnsConflict()
    {
        var jobId = await SeedJobPostingAsync();
        var candidateId = await SeedCandidateAsync();

        var request = new SubmitApplicationRequest(candidateId, null, null, null);

        await _client.PostAsJsonAsync($"/v1/jobs/{jobId}/applications", request);
        var response = await _client.PostAsJsonAsync($"/v1/jobs/{jobId}/applications", request);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetApplication_ReturnsApplication()
    {
        var jobId = await SeedJobPostingAsync();
        var candidateId = await SeedCandidateAsync();
        var request = new SubmitApplicationRequest(candidateId, "Test cover letter", null, null);
        var createResponse = await _client.PostAsJsonAsync($"/v1/jobs/{jobId}/applications", request);
        var created = await createResponse.Content.ReadFromJsonAsync<ApplicationResponse>();

        var response = await _client.GetAsync($"/v1/applications/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var app = await response.Content.ReadFromJsonAsync<ApplicationResponse>();
        app!.CoverLetter.Should().Be("Test cover letter");
    }

    [Fact]
    public async Task GetApplication_NotFound_Returns404()
    {
        var response = await _client.GetAsync($"/v1/applications/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateStatus_ValidTransition_ReturnsOk()
    {
        var jobId = await SeedJobPostingAsync();
        var candidateId = await SeedCandidateAsync();
        var request = new SubmitApplicationRequest(candidateId, null, null, null);
        var createResponse = await _client.PostAsJsonAsync($"/v1/jobs/{jobId}/applications", request);
        var created = await createResponse.Content.ReadFromJsonAsync<ApplicationResponse>();

        var statusRequest = new UpdateApplicationStatusRequest(
            ApplicationStatus.UnderReview, "Qualified candidate", Guid.NewGuid());

        var response = await _client.PatchAsJsonAsync($"/v1/applications/{created!.Id}/status", statusRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<ApplicationResponse>();
        updated!.Status.Should().Be(ApplicationStatus.UnderReview);
        updated.StatusHistory.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateStatus_InvalidTransition_Returns422()
    {
        var jobId = await SeedJobPostingAsync();
        var candidateId = await SeedCandidateAsync();
        var request = new SubmitApplicationRequest(candidateId, null, null, null);
        var createResponse = await _client.PostAsJsonAsync($"/v1/jobs/{jobId}/applications", request);
        var created = await createResponse.Content.ReadFromJsonAsync<ApplicationResponse>();

        var statusRequest = new UpdateApplicationStatusRequest(
            ApplicationStatus.Placed, "Skip all steps", Guid.NewGuid());

        var response = await _client.PatchAsJsonAsync($"/v1/applications/{created!.Id}/status", statusRequest);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task WithdrawApplication_ReturnsNoContent()
    {
        var jobId = await SeedJobPostingAsync();
        var candidateId = await SeedCandidateAsync();
        var request = new SubmitApplicationRequest(candidateId, null, null, null);
        var createResponse = await _client.PostAsJsonAsync($"/v1/jobs/{jobId}/applications", request);
        var created = await createResponse.Content.ReadFromJsonAsync<ApplicationResponse>();

        var response = await _client.DeleteAsync($"/v1/applications/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ListApplicationsByJob_ReturnsList()
    {
        var jobId = await SeedJobPostingAsync();
        var candidateId1 = await SeedCandidateAsync();
        var candidateId2 = await SeedCandidateAsync();
        await _client.PostAsJsonAsync($"/v1/jobs/{jobId}/applications", new SubmitApplicationRequest(candidateId1, null, null, null));
        await _client.PostAsJsonAsync($"/v1/jobs/{jobId}/applications", new SubmitApplicationRequest(candidateId2, null, null, null));

        var response = await _client.GetAsync($"/v1/jobs/{jobId}/applications");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.Content.ReadFromJsonAsync<PaginatedList<ApplicationListItem>>();
        paged.Should().NotBeNull();
        paged!.Items.Should().HaveCount(2);
        paged.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task ListApplicationsByJob_ReturnsListWithNames()
    {
        var jobId = await SeedJobPostingAsync("Diesel Fitter");
        var candidateId = await SeedCandidateAsync("Jack", "Harris");
        await _client.PostAsJsonAsync($"/v1/jobs/{jobId}/applications", new SubmitApplicationRequest(candidateId, null, null, null));

        var response = await _client.GetAsync($"/v1/jobs/{jobId}/applications");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.Content.ReadFromJsonAsync<PaginatedList<ApplicationListItem>>();
        paged.Should().NotBeNull();
        paged!.Items.Should().HaveCount(1);
        paged.Items[0].JobTitle.Should().Be("Diesel Fitter");
        paged.Items[0].CandidateName.Should().Be("Jack Harris");
    }

    [Fact]
    public async Task ListApplicationsByCandidate_ReturnsList()
    {
        var jobId1 = await SeedJobPostingAsync("Job A");
        var jobId2 = await SeedJobPostingAsync("Job B");
        var candidateId = await SeedCandidateAsync("Test", "User");
        await _client.PostAsJsonAsync($"/v1/jobs/{jobId1}/applications", new SubmitApplicationRequest(candidateId, null, null, null));
        await _client.PostAsJsonAsync($"/v1/jobs/{jobId2}/applications", new SubmitApplicationRequest(candidateId, null, null, null));

        var response = await _client.GetAsync($"/v1/candidates/{candidateId}/applications");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.Content.ReadFromJsonAsync<PaginatedList<ApplicationListItem>>();
        paged.Should().NotBeNull();
        paged!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task ListJobs_ReturnsJobs()
    {
        var jobId = await SeedJobPostingAsync("Test Job");

        var response = await _client.GetAsync("/v1/jobs");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.Content.ReadFromJsonAsync<PaginatedList<JobListItem>>();
        paged.Should().NotBeNull();
        var job = paged!.Items.First(j => j.Id == jobId);
        job.Title.Should().Be("Test Job");
    }

    [Fact]
    public async Task ListCandidates_ReturnsCandidates()
    {
        var candidateId = await SeedCandidateAsync("Jane", "Doe");

        var response = await _client.GetAsync("/v1/candidates");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.Content.ReadFromJsonAsync<PaginatedList<CandidateListItem>>();
        paged.Should().NotBeNull();
        var candidate = paged!.Items.First(c => c.Id == candidateId);
        candidate.FirstName.Should().Be("Jane");
        candidate.LastName.Should().Be("Doe");
    }

    [Fact]
    public async Task ListDocuments_ReturnsAllDocuments()
    {
        var (appId1, _) = await SeedApplicationWithDocumentsAsync(2, "docA");
        var (appId2, _) = await SeedApplicationWithDocumentsAsync(1, "docB");

        var response = await _client.GetAsync("/v1/documents");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.Content.ReadFromJsonAsync<PaginatedList<DocumentListItem>>();
        paged.Should().NotBeNull();
        paged!.Items.Should().Contain(d => d.FileName == "docA1.pdf");
        paged.Items.Should().Contain(d => d.FileName == "docA2.pdf");
        paged.Items.Should().Contain(d => d.FileName == "docB1.pdf");
    }

    [Fact]
    public async Task ListDocumentsByApplication_ReturnsApplicationDocuments()
    {
        var (appId, candidateName) = await SeedApplicationWithDocumentsAsync(2, "cv");

        var response = await _client.GetAsync($"/v1/applications/{appId}/documents");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.Content.ReadFromJsonAsync<PaginatedList<DocumentListItem>>();
        paged.Should().NotBeNull();
        paged!.Items.Should().HaveCount(2);
        paged.Items.Should().AllSatisfy(d => d.ApplicationId.Should().Be(appId));
        paged.Items.Should().AllSatisfy(d => d.CandidateName.Should().Be(candidateName));
        paged.Items.Should().Contain(d => d.FileName == "cv1.pdf");
        paged.Items.Should().Contain(d => d.FileName == "cv2.pdf");
    }

    [Fact]
    public async Task ListDocuments_RespectsPagination()
    {
        var response = await _client.GetAsync("/v1/documents?page=1&pageSize=3");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.Content.ReadFromJsonAsync<PaginatedList<DocumentListItem>>();
        paged.Should().NotBeNull();
        paged!.Items.Count.Should().BeLessThanOrEqualTo(3);
        paged.Page.Should().Be(1);
        paged.PageSize.Should().Be(3);
    }

    [Fact]
    public async Task ListDocumentsByApplication_NotFound_ReturnsEmptyList()
    {
        var response = await _client.GetAsync($"/v1/applications/{Guid.NewGuid()}/documents");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.Content.ReadFromJsonAsync<PaginatedList<DocumentListItem>>();
        paged.Should().NotBeNull();
        paged!.Items.Should().BeEmpty();
    }

    private async Task<(Guid appId, string candidateName)> SeedApplicationWithDocumentsAsync(int documentCount, string prefix = "doc")
    {
        var jobId = await SeedJobPostingAsync("Test Job");
        var candidateId = await SeedCandidateAsync("Test", "Candidate");
        var request = new SubmitApplicationRequest(candidateId, "Cover letter", null, null);
        var createResponse = await _client.PostAsJsonAsync($"/v1/jobs/{jobId}/applications", request);
        var created = await createResponse.Content.ReadFromJsonAsync<ApplicationResponse>();

        var appRepo = _factory.Services.GetRequiredService<WorkPac.Recruitment.Shared.Interfaces.IApplicationRepository>();
        var app = await appRepo.GetByIdAsync(created!.Id);

        for (var i = 1; i <= documentCount; i++)
        {
            app!.AddDocument(new DocumentReference
            {
                FileName = $"{prefix}{i}.pdf",
                ContentType = "application/pdf",
                SizeBytes = 1024 * i,
                BlobPath = $"applications/{app.Id}/{prefix}{i}.pdf"
            });
        }
        await appRepo.UpdateAsync(app!);

        return (app!.Id, "Test Candidate");
    }

    private async Task<Guid> SeedJobPostingAsync(string title = "Diesel Fitter")
    {
        var job = JobPosting.Create(
            Guid.NewGuid(), title, "Heavy diesel fitting", JobCategory.Mining,
            new Location { SiteName = "Peak Downs", State = "QLD" },
            new PayRate { Amount = 65, Currency = "AUD", Interval = RateInterval.Hourly },
            EmploymentType.Contract, 5, Guid.NewGuid());

        var jobRepo = _factory.Services.GetRequiredService<WorkPac.Recruitment.Shared.Interfaces.IJobPostingRepository>();
        await jobRepo.AddAsync(job);
        return job.Id;
    }

    private async Task<Guid> SeedCandidateAsync(string firstName = "John", string lastName = "Smith")
    {
        var candidate = Candidate.Create(
            firstName, lastName, $"john.{Guid.NewGuid():N}@example.com", null,
            new Location { Suburb = "Brisbane", State = "QLD" });

        var candidateRepo = _factory.Services.GetRequiredService<WorkPac.Recruitment.Shared.Interfaces.ICandidateRepository>();
        await candidateRepo.AddAsync(candidate);
        return candidate.Id;
    }
}
