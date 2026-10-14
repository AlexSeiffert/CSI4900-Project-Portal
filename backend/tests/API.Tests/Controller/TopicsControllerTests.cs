using API.Controllers;
using API.Exceptions;
using Microsoft.AspNetCore.Mvc;
using API.Models;
using System.Net;
using System.Net.Http.Json;
using API.DTOs.Request;
using API.Tests.Support;
using static API.Tests.Support.ApiTestData;

namespace API.Tests.Controller;

public class TopicsControllerTests
{
    [Fact]
    public async Task GetTopics_ReturnsSeededTopicsThroughHttp()
    {
        await using var factory = new PortalApiFactory();
        using var client = await factory.CreateSeededClient();
        var topics = await Read<List<DTOs.Topic>>(await client.GetAsync("/api/v1/topics"));
        Assert.Equal(2, topics.Count);
        Assert.Contains(topics, topic => topic.Id == PortalApiFactory.TopicId);
        Assert.Contains(topics, topic => topic.Id == PortalApiFactory.OtherTopicId);
    }

    [Fact]
    public async Task GetTopics_CallsMatchingServiceMethodAndReturnsExpectedResult()
    {
        var service = new TopicServiceStub();
        service.Items.Add(service.Item);
        var controller = new TopicsController(service);
        using var cancellation = new CancellationTokenSource();
        var result = await controller.GetTopics(cancellation.Token);
        Assert.Same(service.Items, Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal("GetTopics", service.LastMethod);
        Assert.Equal(cancellation.Token, service.LastToken);
        Assert.Empty(service.LastArguments);
    }

    [Theory]
    [InlineData(ErrorKind.NotFound)]
    [InlineData(ErrorKind.Validation)]
    [InlineData(ErrorKind.Conflict)]
    [InlineData(ErrorKind.Cancelled)]
    [InlineData(ErrorKind.Unexpected)]
    public async Task GetTopics_PropagatesServiceExceptionsToTheHandler(ErrorKind kind)
    {
        var service = new TopicServiceStub();
        service.Items.Add(service.Item);
        var controller = new TopicsController(service);
        using var cancellation = new CancellationTokenSource();
        var original = new ServiceException("GetTopics", kind, "Service failed", new InvalidOperationException("Original cause"));
        service.Failure = original;
        var exception = await Assert.ThrowsAsync<ServiceException>(() => controller.GetTopics(cancellation.Token));
        Assert.Same(original, exception);
        Assert.Equal(cancellation.Token, service.LastToken);
    }
}
