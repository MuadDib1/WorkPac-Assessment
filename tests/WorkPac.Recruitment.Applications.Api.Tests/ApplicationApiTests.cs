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
        var list = await response.Content.ReadFromJsonAsync<List<ApplicationListItem>>();
        list.Should().HaveCount(2);
    }

    [Fact]
    public async Task ListApplicationsByJob_ReturnsListWithNames()
    {
        var jobId = await SeedJobPostingAsync("Diesel Fitter");
        var candidateId = await SeedCandidateAsync("Jack", "Harris");
        await _client.PostAsJsonAsync($"/v1/jobs/{jobId}/applications", new SubmitApplicationRequest(candidateId, null, null, null));

        var response = await _client.GetAsync($"/v1/jobs/{jobId}/applications");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<List<ApplicationListItem>>();
        list.Should().HaveCount(1);
        list![0].JobTitle.Should().Be("Diesel Fitter");
        list[0].CandidateName.Should().Be("Jack Harris");
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
        var list = await response.Content.ReadFromJsonAsync<List<ApplicationListItem>>();
        list.Should().HaveCount(2);
    }

    [Fact]
    public async Task ListJobs_ReturnsJobs()
    {
        var jobId = await SeedJobPostingAsync("Test Job");

        var response = await _client.GetAsync("/v1/jobs");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<List<JobListItem>>();
        list.Should().NotBeNull();
        var job = list!.First(j => j.Id == jobId);
        job.Title.Should().Be("Test Job");
    }

    [Fact]
    public async Task ListCandidates_ReturnsCandidates()
    {
        var candidateId = await SeedCandidateAsync("Jane", "Doe");

        var response = await _client.GetAsync("/v1/candidates");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<List<CandidateListItem>>();
        list.Should().NotBeNull();
        var candidate = list!.First(c => c.Id == candidateId);
        candidate.FirstName.Should().Be("Jane");
        candidate.LastName.Should().Be("Doe");
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
