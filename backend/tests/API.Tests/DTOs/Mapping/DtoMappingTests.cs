using API.DTOs.Mapping;
using API.Models;

namespace API.Tests.DTOs.Mapping;

public class DtoMappingTests
{
    [Fact]
    public void ToDto_MapsTopic()
    {
        var model = new Topic
        {
            Id = Guid.NewGuid(),
            Name = "Databases"
        };

        var result = model.ToDto();

        Assert.Equal(model.Id, result.Id);
        Assert.Equal(model.Name, result.Name);
    }

    [Fact]
    public void ToDto_MapsProject()
    {
        var topic = new Topic { Id = Guid.NewGuid(), Name = "Databases" };
        var model = new Project
        {
            Id = Guid.NewGuid(),
            SupervisorId = Guid.NewGuid(),
            Title = "Portal",
            Description = "Research project",
            ExternalUrl = "https://example.test/project",
            Status = ProjectStatus.Archived,
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            Topics = [topic]
        };

        var result = model.ToDto();

        Assert.Equal(model.Id, result.Id);
        Assert.Equal(model.SupervisorId, result.SupervisorId);
        Assert.Equal(model.Title, result.Title);
        Assert.Equal(model.Description, result.Description);
        Assert.Equal(model.ExternalUrl, result.ExternalUrl);
        Assert.Equal(model.Status, result.Status);
        Assert.Equal(model.CreatedAt, result.CreatedAt);
        Assert.Equal(model.UpdatedAt, result.UpdatedAt);
        var mappedTopic = Assert.Single(result.Topics);
        Assert.Equal(topic.Id, mappedTopic.Id);
        Assert.Equal(topic.Name, mappedTopic.Name);
    }

    [Fact]
    public void ToDto_MapsInquiry()
    {
        var model = new Inquiry
        {
            Id = Guid.NewGuid(),
            StudentId = Guid.NewGuid(),
            SupervisorId = Guid.NewGuid(),
            ProjectId = Guid.NewGuid(),
            Message = "Interested in this project",
            CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        var result = model.ToDto();

        Assert.Equal(model.Id, result.Id);
        Assert.Equal(model.StudentId, result.StudentId);
        Assert.Equal(model.SupervisorId, result.SupervisorId);
        Assert.Equal(model.ProjectId, result.ProjectId);
        Assert.Equal(model.Message, result.Message);
        Assert.Equal(model.CreatedAt, result.CreatedAt);
        Assert.Equal(model.UpdatedAt, result.UpdatedAt);
    }

    [Fact]
    public void ToDto_MapsStudent()
    {
        var topic = new Topic { Id = Guid.NewGuid(), Name = "Databases" };
        var model = new Student
        {
            UserId = Guid.NewGuid(),
            DisplayName = "Student",
            Program = "Computer Science",
            UpdatedAt = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            TopicInterests = [topic]
        };

        var result = model.ToDto();

        Assert.Equal(model.UserId, result.UserId);
        Assert.Equal(model.DisplayName, result.DisplayName);
        Assert.Equal(model.Program, result.Program);
        Assert.Equal(model.UpdatedAt, result.UpdatedAt);
        var mappedTopic = Assert.Single(result.TopicInterests);
        Assert.Equal(topic.Id, mappedTopic.Id);
        Assert.Equal(topic.Name, mappedTopic.Name);
    }

    [Fact]
    public void ToDto_MapsSupervisor()
    {
        var topic = new Topic { Id = Guid.NewGuid(), Name = "Databases" };
        var model = new Supervisor
        {
            UserId = Guid.NewGuid(),
            DisplayName = "Supervisor",
            ContactEmail = "supervisor@example.test",
            AboutMe = "Database research",
            ExternalUrl = "https://example.test/supervisor",
            AcceptingInquiries = false,
            UpdatedAt = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc),
            TopicInterests = [topic]
        };

        var result = model.ToDto();

        Assert.Equal(model.UserId, result.UserId);
        Assert.Equal(model.DisplayName, result.DisplayName);
        Assert.Equal(model.ContactEmail, result.ContactEmail);
        Assert.Equal(model.AboutMe, result.AboutMe);
        Assert.Equal(model.ExternalUrl, result.ExternalUrl);
        Assert.Equal(model.AcceptingInquiries, result.AcceptingInquiries);
        Assert.Equal(model.UpdatedAt, result.UpdatedAt);
        var mappedTopic = Assert.Single(result.TopicInterests);
        Assert.Equal(topic.Id, mappedTopic.Id);
        Assert.Equal(topic.Name, mappedTopic.Name);
    }
}
