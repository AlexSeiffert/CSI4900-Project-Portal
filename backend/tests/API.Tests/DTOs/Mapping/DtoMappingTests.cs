using API.DTOs.Mapping;
using API.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace API.Tests.DTOs.Mapping;

public class DtoMappingTests
{
    [Fact]
    public void ToDto_OmitsLoadedRelationshipsUnlessRequested()
    {
        var supervisor = new Supervisor { UserId = Guid.NewGuid(), DisplayName = "Supervisor" };
        var project = new Project
        {
            Id = Guid.NewGuid(), SupervisorId = supervisor.UserId, Supervisor = supervisor,
            Topics = [new Topic { Id = Guid.NewGuid(), Name = "Databases" }]
        };

        var basic = project.ToDto();
        var detailed = project.ToDtoWithRelations();

        Assert.Null(basic.Supervisor);
        Assert.Null(basic.Topics);
        Assert.Equal(supervisor.UserId, detailed.Supervisor?.UserId);
        Assert.Single(detailed.Topics!);
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(basic));
        Assert.False(json.RootElement.TryGetProperty("Supervisor", out _));
        Assert.False(json.RootElement.TryGetProperty("Topics", out _));
    }

    [Fact]
    public void ToDto_ReusesBackReferencesAndSerializesCycles()
    {
        var student = new Student { UserId = Guid.NewGuid(), DisplayName = "Student" };
        var supervisor = new Supervisor { UserId = Guid.NewGuid(), DisplayName = "Supervisor" };
        var project = new Project { Id = Guid.NewGuid(), SupervisorId = supervisor.UserId, Supervisor = supervisor };
        var topic = new Topic { Id = Guid.NewGuid(), Name = "Databases" };
        var inquiry = new Inquiry
        {
            Id = Guid.NewGuid(), StudentId = student.UserId, SupervisorId = supervisor.UserId,
            ProjectId = project.Id, Student = student, Supervisor = supervisor, Project = project
        };
        student.Inquiries.Add(inquiry);
        student.SavedProjects.Add(project);
        supervisor.Projects.Add(project);
        supervisor.Inquiries.Add(inquiry);
        project.Inquiries.Add(inquiry);
        project.Topics.Add(topic);
        topic.Projects.Add(project);
        topic.Students.Add(student);
        topic.Supervisors.Add(supervisor);

        var result = project.ToDtoWithRelations();

        Assert.Same(result, result.Supervisor?.Projects!.Single());
        Assert.Same(result, result.Inquiries!.Single().Project);
        Assert.Same(result, result.Topics!.Single().Projects!.Single());
        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        });
        using var document = JsonDocument.Parse(json);
        Assert.Equal(project.Id, document.RootElement.GetProperty("Id").GetGuid());
        Assert.Equal(JsonValueKind.Null,
            document.RootElement.GetProperty("Supervisor").GetProperty("Projects")[0].ValueKind);
    }

    [Fact]
    public void ToDto_MapsTopic()
    {
        var model = new Topic
        {
            Id = Guid.NewGuid(),
            Name = "Databases"
        };

        var result = model.ToDtoWithRelations();

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

        var result = model.ToDtoWithRelations();

        Assert.Equal(model.Id, result.Id);
        Assert.Equal(model.SupervisorId, result.SupervisorId);
        Assert.Equal(model.Title, result.Title);
        Assert.Equal(model.Description, result.Description);
        Assert.Equal(model.ExternalUrl, result.ExternalUrl);
        Assert.Equal(model.Status, result.Status);
        Assert.Equal(model.CreatedAt, result.CreatedAt);
        Assert.Equal(model.UpdatedAt, result.UpdatedAt);
        var mappedTopic = Assert.Single(result.Topics!);
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

        var result = model.ToDtoWithRelations();

        Assert.Equal(model.Id, result.Id);
        Assert.Equal(model.StudentId, result.StudentId);
        Assert.Equal(model.SupervisorId, result.SupervisorId);
        Assert.Equal(model.ProjectId, result.ProjectId);
        Assert.Equal(model.Message, result.Message);
        Assert.Equal(model.CreatedAt, result.CreatedAt);
        Assert.Equal(model.UpdatedAt, result.UpdatedAt);
    }

    [Fact]
    public void ToDto_MapsInquiryRelationships()
    {
        var topic = new Topic { Id = Guid.NewGuid(), Name = "Databases" };
        var student = new Student { UserId = Guid.NewGuid(), DisplayName = "Student", TopicInterests = [topic] };
        var supervisor = new Supervisor { UserId = Guid.NewGuid(), DisplayName = "Supervisor", ContactEmail = "supervisor@example.test", TopicInterests = [topic] };
        var project = new Project { Id = Guid.NewGuid(), SupervisorId = supervisor.UserId, Title = "Portal", Topics = [topic] };
        var inquiry = new Inquiry
        {
            Id = Guid.NewGuid(), StudentId = student.UserId, SupervisorId = supervisor.UserId,
            ProjectId = project.Id, Student = student, Supervisor = supervisor, Project = project
        };

        var result = inquiry.ToDtoWithRelations();

        Assert.Equal(student.UserId, result.Student?.UserId);
        Assert.Equal(topic.Id, Assert.Single(result.Student!.TopicInterests!).Id);
        Assert.Equal(supervisor.ContactEmail, result.Supervisor?.ContactEmail);
        Assert.Equal(topic.Id, Assert.Single(result.Supervisor!.TopicInterests!).Id);
        Assert.Equal(project.Title, result.Project?.Title);
        Assert.Equal(topic.Id, Assert.Single(result.Project!.Topics!).Id);
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

        var result = model.ToDtoWithRelations();

        Assert.Equal(model.UserId, result.UserId);
        Assert.Equal(model.DisplayName, result.DisplayName);
        Assert.Equal(model.Program, result.Program);
        Assert.Equal(model.UpdatedAt, result.UpdatedAt);
        var mappedTopic = Assert.Single(result.TopicInterests!);
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

        var result = model.ToDtoWithRelations();

        Assert.Equal(model.UserId, result.UserId);
        Assert.Equal(model.DisplayName, result.DisplayName);
        Assert.Equal(model.ContactEmail, result.ContactEmail);
        Assert.Equal(model.AboutMe, result.AboutMe);
        Assert.Equal(model.ExternalUrl, result.ExternalUrl);
        Assert.Equal(model.AcceptingInquiries, result.AcceptingInquiries);
        Assert.Equal(model.UpdatedAt, result.UpdatedAt);
        var mappedTopic = Assert.Single(result.TopicInterests!);
        Assert.Equal(topic.Id, mappedTopic.Id);
        Assert.Equal(topic.Name, mappedTopic.Name);
    }
}
