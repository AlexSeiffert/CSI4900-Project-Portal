using System.Text.Json;
using System.Text.Json.Serialization;
using API.Models.Data;
using API.Repositories;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Tests.DTOs.Mapping;

public class DtoResponseSerializationTests
{
    [Fact]
    public async Task SeededProjectAndTopicListsSerialize()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using var db = new ApplicationDbContext(options);
        await DevelopmentData.Seed(db);
        db.ChangeTracker.Clear();
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            MaxDepth = 32
        };

        var projects = await new ProjectService(new ProjectRepository(db)).GetProjects();
        var topics = await new TopicService(new TopicRepository(db)).GetTopics();

        Assert.Equal(9, JsonDocument.Parse(JsonSerializer.Serialize(projects, jsonOptions)).RootElement.GetArrayLength());
        Assert.Equal(8, JsonDocument.Parse(JsonSerializer.Serialize(topics, jsonOptions)).RootElement.GetArrayLength());
    }
}
