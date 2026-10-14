using API.Controllers;
using API.DTOs;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace API.Tests.Controller;

public class TopicsControllerTests
{
    private readonly Mock<ITopicService> _service = new();

    private TopicsController CreateController()
    {
        return new(_service.Object);
    }

    [Fact]
    public async Task GetTopics_ReturnsOkWithTopics()
    {
        var topics = new List<Topic> { new() { Id = Guid.NewGuid() } };

        _service.Setup(service => service.GetTopics(CancellationToken.None))
            .ReturnsAsync(topics);
            
        var controller = CreateController();

        var result = await controller.GetTopics(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(topics, ok.Value);
    }
}
