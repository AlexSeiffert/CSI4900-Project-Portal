using API.Models;
using API.Models.Data;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace API.Tests.Support;

internal sealed class RepositoryTestContext : IAsyncDisposable
{
    public Mock<ApplicationDbContext> DbMock { get; }
    public ApplicationDbContext Db => DbMock.Object;
    public Topic Topic { get; } = new() { Id = Guid.NewGuid(), Name = "Zoology" };
    public Topic OtherTopic { get; } = new() { Id = Guid.NewGuid(), Name = "Algorithms" };
    public Student Student { get; } = new() { UserId = Guid.NewGuid(), DisplayName = "Zoe", Program = "CS" };
    public Student OtherStudent { get; } = new() { UserId = Guid.NewGuid(), DisplayName = "Alex" };
    public Supervisor Supervisor { get; } = new() { UserId = Guid.NewGuid(), DisplayName = "Zoe", ContactEmail = "zoe@example.test" };
    public Supervisor OtherSupervisor { get; } = new() { UserId = Guid.NewGuid(), DisplayName = "Alex", ContactEmail = "alex@example.test" };
    public Project Project { get; }
    public Project OtherProject { get; }
    public Inquiry Inquiry { get; }
    public Inquiry OtherInquiry { get; }

    private RepositoryTestContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        DbMock = new Mock<ApplicationDbContext>(options) { CallBase = true };
        Student.TopicInterests.Add(Topic);
        Supervisor.TopicInterests.Add(Topic);
        Project = new() { Id = Guid.NewGuid(), SupervisorId = Supervisor.UserId, Title = "Zoology portal", Description = "Research", Topics = [Topic], CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) };
        OtherProject = new() { Id = Guid.NewGuid(), SupervisorId = OtherSupervisor.UserId, Title = "Algorithms portal", Description = "Other research", Status = ProjectStatus.Archived, Topics = [OtherTopic], CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) };
        Inquiry = new() { Id = Guid.NewGuid(), StudentId = Student.UserId, SupervisorId = Supervisor.UserId, ProjectId = Project.Id, Message = "Original", CreatedAt = Project.CreatedAt };
        OtherInquiry = new() { Id = Guid.NewGuid(), StudentId = OtherStudent.UserId, SupervisorId = OtherSupervisor.UserId, ProjectId = OtherProject.Id, CreatedAt = OtherProject.CreatedAt };
    }

    public static async Task<RepositoryTestContext> CreateAsync()
    {
        var context = new RepositoryTestContext();
        context.Db.AddRange(context.Student, context.OtherStudent, context.Supervisor, context.OtherSupervisor,
            context.Topic, context.OtherTopic, context.Project, context.OtherProject, context.Inquiry, context.OtherInquiry);
        await context.Db.SaveChangesAsync();
        context.Db.ChangeTracker.Clear();
        context.DbMock.Invocations.Clear();
        return context;
    }

    public ValueTask DisposeAsync()
    {
        return Db.DisposeAsync();
    }
}
