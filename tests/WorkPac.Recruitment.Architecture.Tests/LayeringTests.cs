using FluentAssertions;
using NetArchTest.Rules;

namespace WorkPac.Recruitment.Architecture.Tests;

public class LayeringTests
{
    private const string SharedNamespace = "WorkPac.Recruitment.Shared";
    private const string ContractsNamespace = "WorkPac.Recruitment.Contracts";
    private const string InfrastructureNamespace = "WorkPac.Recruitment.Infrastructure";
    private const string ApplicationsApiNamespace = "WorkPac.Recruitment.Applications.Api";
    private const string MatchingServiceNamespace = "WorkPac.Recruitment.Matching.Service";

    [Fact]
    public void Shared_ShouldNotDependOn_AnyOtherProject()
    {
        var result = Types.InAssembly(typeof(Shared.BaseEntity).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                ContractsNamespace,
                InfrastructureNamespace,
                ApplicationsApiNamespace,
                MatchingServiceNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Contracts_ShouldNotDependOn_InfrastructureOrServices()
    {
        var result = Types.InAssembly(typeof(Contracts.ApiModels.SubmitApplicationRequest).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                InfrastructureNamespace,
                ApplicationsApiNamespace,
                MatchingServiceNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Infrastructure_ShouldNotDependOn_Services()
    {
        var result = Types.InAssembly(typeof(Infrastructure.Persistence.RecruitmentDbContext).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                ApplicationsApiNamespace,
                MatchingServiceNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void DomainTypes_ShouldBePublic()
    {
        var result = Types.InAssembly(typeof(Shared.Domain.Application).Assembly)
            .That()
            .ResideInNamespace("WorkPac.Recruitment.Shared.Domain")
            .Should()
            .BePublic()
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void SharedAndContracts_ShouldHaveNamespacesMatchingAssemblyName()
    {
        var assemblies = new[]
        {
            typeof(Shared.BaseEntity).Assembly,
            typeof(Contracts.ApiModels.SubmitApplicationRequest).Assembly
        };

        foreach (var assembly in assemblies)
        {
            var prefix = assembly.GetName().Name!;
            foreach (var type in assembly.GetExportedTypes())
            {
                type.Namespace.Should().StartWith(prefix,
                    $"{type.FullName} should be in a namespace starting with {prefix}");
            }
        }
    }
}
