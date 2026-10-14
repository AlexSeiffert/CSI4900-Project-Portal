using API.DTOs.Request;
using API.Exceptions;
using API.Models;
using API.Tests.Support;

namespace API.Tests.Service;

public class TopicServiceTests
{
    [Theory]
    [MemberData(nameof(ServiceFailureCases.All), MemberType = typeof(ServiceFailureCases))]
    public async Task GetTopics_PreservesDependencyFailureAndClassification(string kind)
    {
        var c = new ServiceTestContext();
        using var cancellation = new CancellationTokenSource();
        var token = cancellation.Token;
        var original = ServiceFailureCases.Create(kind);
        c.Topics.Failure = original;
        c.Topics.FailureMethod = "GetTopics";
        await ServiceFailureCases.AssertWrapped(() => c.TopicService.GetTopics(token), "GetTopics", original);
        Assert.Equal(token, c.Topics.LastToken);
    }

    [Fact]
    public async Task GetTopics_ReturnsMappedTopics()
    {
        var c = new ServiceTestContext();
        var result = Assert.Single(await c.TopicService.GetTopics());
        Assert.Equal(c.Topic.Id, result.Id);
        Assert.Equal(c.Topic.Name, result.Name);
        Assert.Equal("GetTopics", Assert.Single(c.Topics.Calls));
    }

    [Fact]
    public async Task GetTopics_ReturnsEmptyCollectionWhenNoTopicsExist()
    {
        var c = new ServiceTestContext();
        c.Topics.Items.Clear();
        Assert.Empty(await c.TopicService.GetTopics());
    }

    [Theory]
    [MemberData(nameof(ServiceFailureCases.All), MemberType = typeof(ServiceFailureCases))]
    public async Task ValidateTopicsExist_PreservesDependencyFailureAndClassification(string kind)
    {
        var c = new ServiceTestContext();
        using var cancellation = new CancellationTokenSource();
        var original = ServiceFailureCases.Create(kind);
        c.Topics.Failure = original;
        await ServiceFailureCases.AssertWrapped(() => c.TopicService.ValidateTopicsExist(new[] { c.Topic.Id, c.Topic.Id }, cancellation.Token), "ValidateTopicsExist", original);
        Assert.Equal(cancellation.Token, c.Topics.LastToken);
    }

    [Fact]
    public async Task ValidateTopicsExist_AcceptsExistingReferences()
    {
        var c = new ServiceTestContext();
        await c.TopicService.ValidateTopicsExist(new[] { c.Topic.Id, c.Topic.Id });
        Assert.Equal("GetTopicsByIds", Assert.Single(c.Topics.Calls));
    }

    [Fact]
    public async Task ValidateTopicsExist_RejectsMissingReferences()
    {
        var c = new ServiceTestContext();
        await ServiceFailureCases.AssertDomainFailure(() => c.TopicService.ValidateTopicsExist(new[] { Guid.NewGuid() }), "ValidateTopicsExist", ErrorType.Validation);
    }

    [Fact]
    public async Task ValidateTopicsExist_AcceptsEmptySelection()
    {
        var c = new ServiceTestContext();
        await c.TopicService.ValidateTopicsExist([]);
        Assert.Equal("GetTopicsByIds", Assert.Single(c.Topics.Calls));
    }

    [Fact]
    public async Task ValidateTopicsExist_RejectsNullSelectionBeforeRepositoryCall()
    {
        var c = new ServiceTestContext();
        await ServiceFailureCases.AssertDomainFailure(() => c.TopicService.ValidateTopicsExist(null!), "ValidateTopicsExist", ErrorType.Validation);
        Assert.Empty(c.Topics.Calls);
    }

    [Fact]
    public void Constructor_DependsOnlyOnOwnRepository()
    {
        var constructor = Assert.Single(typeof(API.Services.TopicService).GetConstructors());
        var dependency = Assert.Single(constructor.GetParameters());
        Assert.Equal(typeof(API.Repositories.Interfaces.ITopicRepository), dependency.ParameterType);
    }
}
