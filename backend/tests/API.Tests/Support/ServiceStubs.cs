using API.DTOs;
using API.DTOs.Request;
using API.Models;
using API.Services.Interfaces;

namespace API.Tests.Support;

internal abstract class ServiceStub
{
    public Exception? Failure { get; set; }
    public Action<string>? OnCall { get; set; }
    public string? LastMethod { get; private set; }
    public List<string> Calls { get; } = [];
    public object?[] LastArguments { get; private set; } = [];
    public CancellationToken LastToken { get; private set; }

    protected Task<T> Complete<T>(string method, CancellationToken token, T result, params object?[] arguments)
    {
        OnCall?.Invoke(method);
        Calls.Add(method);
        LastMethod = method;
        LastArguments = arguments;
        LastToken = token;
        return Failure == null ? Task.FromResult(result) : Task.FromException<T>(Failure);
    }

    protected Task CompleteCommand(string method, CancellationToken token, params object?[] arguments)
    {
        OnCall?.Invoke(method);
        Calls.Add(method);
        LastMethod = method;
        LastArguments = arguments;
        LastToken = token;
        return Failure == null ? Task.CompletedTask : Task.FromException(Failure);
    }
}

internal sealed class ProjectServiceStub : ServiceStub, IProjectService
{
    public DTOs.Project Item { get; } = new() { Id = Guid.NewGuid() };
    public List<DTOs.Project> Items { get; } = [];

    public Task<IEnumerable<DTOs.Project>> GetProjects(CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetProjects), cancellationToken, Items.AsEnumerable());
    }

    public Task<IEnumerable<DTOs.Project>> GetProjectsBySupervisorId(Guid supervisorId, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetProjectsBySupervisorId), cancellationToken, Items.AsEnumerable(), supervisorId);
    }

    public Task<IEnumerable<DTOs.Project>> GetProjectsByTopicId(Guid topicId, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetProjectsByTopicId), cancellationToken, Items.AsEnumerable(), topicId);
    }

    public Task<IEnumerable<DTOs.Project>> GetProjectsByStatus(ProjectStatus status, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetProjectsByStatus), cancellationToken, Items.AsEnumerable(), status);
    }

    public Task<DTOs.Project> GetProjectById(Guid projectId, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetProjectById), cancellationToken, Item, projectId);
    }

    public Task<DTOs.Project> CreateProject(CreateProject request, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(CreateProject), cancellationToken, Item, request);
    }

    public Task<DTOs.Project> UpdateProject(Guid projectId, UpdateProject request, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(UpdateProject), cancellationToken, Item, projectId, request);
    }

    public Task DeleteProject(Guid projectId, CancellationToken cancellationToken = default)
    {
        return CompleteCommand(nameof(DeleteProject), cancellationToken, projectId);
    }
    public Task ValidateProjectSupervisor(Guid? projectId, Guid supervisorId, CancellationToken cancellationToken = default)
    {
        return CompleteCommand(nameof(ValidateProjectSupervisor), cancellationToken, projectId, supervisorId);
    }

}

internal sealed class InquiryServiceStub : ServiceStub, IInquiryService
{
    public DTOs.Inquiry Item { get; } = new() { Id = Guid.NewGuid() };
    public List<DTOs.Inquiry> Items { get; } = [];

    public Task<IEnumerable<DTOs.Inquiry>> GetInquiries(CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetInquiries), cancellationToken, Items.AsEnumerable());
    }

    public Task<IEnumerable<DTOs.Inquiry>> GetInquiriesByStudentId(Guid studentId, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetInquiriesByStudentId), cancellationToken, Items.AsEnumerable(), studentId);
    }

    public Task<IEnumerable<DTOs.Inquiry>> GetInquiriesBySupervisorId(Guid supervisorId, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetInquiriesBySupervisorId), cancellationToken, Items.AsEnumerable(), supervisorId);
    }

    public Task<IEnumerable<DTOs.Inquiry>> GetInquiriesByProjectId(Guid projectId, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetInquiriesByProjectId), cancellationToken, Items.AsEnumerable(), projectId);
    }

    public Task<DTOs.Inquiry> GetInquiryById(Guid inquiryId, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetInquiryById), cancellationToken, Item, inquiryId);
    }

    public Task<DTOs.Inquiry> CreateInquiry(CreateInquiry request, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(CreateInquiry), cancellationToken, Item, request);
    }

    public Task<DTOs.Inquiry> UpdateInquiry(Guid inquiryId, UpdateInquiry request, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(UpdateInquiry), cancellationToken, Item, inquiryId, request);
    }
}

internal sealed class StudentServiceStub : ServiceStub, IStudentService
{
    public DTOs.Student Item { get; } = new() { UserId = Guid.NewGuid() };
    public List<DTOs.Student> Items { get; } = [];

    public Task<IEnumerable<DTOs.Student>> GetStudents(CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetStudents), cancellationToken, Items.AsEnumerable());
    }

    public Task<DTOs.Student> GetStudentById(Guid userId, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetStudentById), cancellationToken, Item, userId);
    }

    public Task<DTOs.Student> UpdateStudent(Guid userId, UpdateStudent request, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(UpdateStudent), cancellationToken, Item, userId, request);
    }
    public Task ValidateStudentExists(Guid studentId, CancellationToken cancellationToken = default)
    {
        return CompleteCommand(nameof(ValidateStudentExists), cancellationToken, studentId);
    }

}

internal sealed class SupervisorServiceStub : ServiceStub, ISupervisorService
{
    public DTOs.Supervisor Item { get; } = new() { UserId = Guid.NewGuid() };
    public List<DTOs.Supervisor> Items { get; } = [];

    public Task<IEnumerable<DTOs.Supervisor>> GetSupervisors(CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetSupervisors), cancellationToken, Items.AsEnumerable());
    }

    public Task<DTOs.Supervisor> GetSupervisorById(Guid userId, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetSupervisorById), cancellationToken, Item, userId);
    }

    public Task<DTOs.Supervisor> UpdateSupervisor(Guid userId, UpdateSupervisor request, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(UpdateSupervisor), cancellationToken, Item, userId, request);
    }
    public Task ValidateSupervisorExists(Guid supervisorId, CancellationToken cancellationToken = default)
    {
        return CompleteCommand(nameof(ValidateSupervisorExists), cancellationToken, supervisorId);
    }

}

internal sealed class TopicServiceStub : ServiceStub, ITopicService
{
    public DTOs.Topic Item { get; } = new() { Id = Guid.NewGuid() };
    public List<DTOs.Topic> Items { get; } = [];

    public Task<IEnumerable<DTOs.Topic>> GetTopics(CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetTopics), cancellationToken, Items.AsEnumerable());
    }
    public Task ValidateTopicsExist(IReadOnlyCollection<Guid> topicIds, CancellationToken cancellationToken = default)
    {
        return CompleteCommand(nameof(ValidateTopicsExist), cancellationToken, topicIds);
    }

}

internal sealed class SavedProjectServiceStub : ServiceStub, ISavedProjectService
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
