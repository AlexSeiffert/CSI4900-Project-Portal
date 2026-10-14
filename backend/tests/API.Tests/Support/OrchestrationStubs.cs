using API.DTOs.Request;
using API.Orchestrations.Interfaces;

namespace API.Tests.Support;

internal sealed class ProjectOrchestrationStub : ServiceStub, IProjectOrchestration
{
    public DTOs.Project Item { get; } = new() { Id = Guid.NewGuid() };
    public List<DTOs.Project> Items { get; } = [];

    public Task<DTOs.Project> CreateProject(CreateProject request, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(CreateProject), cancellationToken, Item, request);
    }

    public Task<DTOs.Project> UpdateProject(Guid projectId, UpdateProject request, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(UpdateProject), cancellationToken, Item, projectId, request);
    }
}

internal sealed class StudentOrchestrationStub : ServiceStub, IStudentOrchestration
{
    public DTOs.Student Item { get; } = new() { UserId = Guid.NewGuid() };
    public List<DTOs.Student> Items { get; } = [];

    public Task<DTOs.Student> UpdateStudent(Guid userId, UpdateStudent request, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(UpdateStudent), cancellationToken, Item, userId, request);
    }
}

internal sealed class SupervisorOrchestrationStub : ServiceStub, ISupervisorOrchestration
{
    public DTOs.Supervisor Item { get; } = new() { UserId = Guid.NewGuid() };
    public List<DTOs.Supervisor> Items { get; } = [];

    public Task<DTOs.Supervisor> UpdateSupervisor(Guid userId, UpdateSupervisor request, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(UpdateSupervisor), cancellationToken, Item, userId, request);
    }
}

internal sealed class InquiryOrchestrationStub : ServiceStub, IInquiryOrchestration
{
    public DTOs.Inquiry Item { get; } = new() { Id = Guid.NewGuid() };
    public List<DTOs.Inquiry> Items { get; } = [];

    public Task<DTOs.Inquiry> CreateInquiry(CreateInquiry request, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(CreateInquiry), cancellationToken, Item, request);
    }
}

internal sealed class SavedProjectOrchestrationStub : ServiceStub, ISavedProjectOrchestration
{
    public DTOs.Project Item { get; } = new() { Id = Guid.NewGuid() };
    public List<DTOs.Project> Items { get; } = [];

    public Task<IEnumerable<DTOs.Project>> GetSavedProjects(Guid studentId, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetSavedProjects), cancellationToken, Items.AsEnumerable(), studentId);
    }

    public Task SaveProject(Guid studentId, Guid projectId, CancellationToken cancellationToken = default)
    {
        return CompleteCommand(nameof(SaveProject), cancellationToken, studentId, projectId);
    }

    public Task RemoveSavedProject(Guid studentId, Guid projectId, CancellationToken cancellationToken = default)
    {
        return CompleteCommand(nameof(RemoveSavedProject), cancellationToken, studentId, projectId);
    }
}
