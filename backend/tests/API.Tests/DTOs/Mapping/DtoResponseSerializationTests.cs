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
        var inquiries = await new InquiryService(new InquiryRepository(db)).GetInquiries();
        var students = await new StudentService(new StudentRepository(db)).GetStudents();
        var supervisors = await new SupervisorService(new SupervisorRepository(db)).GetSupervisors();

        using var projectJson = JsonDocument.Parse(JsonSerializer.Serialize(projects, jsonOptions));
        using var topicJson = JsonDocument.Parse(JsonSerializer.Serialize(topics, jsonOptions));
        using var inquiryJson = JsonDocument.Parse(JsonSerializer.Serialize(inquiries, jsonOptions));
        using var studentJson = JsonDocument.Parse(JsonSerializer.Serialize(students, jsonOptions));
        using var supervisorJson = JsonDocument.Parse(JsonSerializer.Serialize(supervisors, jsonOptions));
        Assert.Equal(9, projectJson.RootElement.GetArrayLength());
        Assert.Equal(8, topicJson.RootElement.GetArrayLength());
        Assert.False(projectJson.RootElement[0].TryGetProperty("topics", out _));
        Assert.False(projectJson.RootElement[0].TryGetProperty("supervisor", out _));
        Assert.False(topicJson.RootElement[0].TryGetProperty("projects", out _));
        Assert.False(inquiryJson.RootElement[0].TryGetProperty("student", out _));
        Assert.False(studentJson.RootElement[0].TryGetProperty("topicInterests", out _));
        Assert.False(supervisorJson.RootElement[0].TryGetProperty("projects", out _));

        var projectId = Guid.Parse("44444444-4444-4444-4444-444444444441");
        var detail = await new ProjectService(new ProjectRepository(db)).GetProjectById(projectId);
        using var detailJson = JsonDocument.Parse(JsonSerializer.Serialize(detail, jsonOptions));
        Assert.True(detailJson.RootElement.TryGetProperty("topics", out _));
        Assert.True(detailJson.RootElement.TryGetProperty("supervisor", out _));
    }
}
