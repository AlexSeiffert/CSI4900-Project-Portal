using API.Models;
using API.Repositories.Interfaces;

namespace API.Tests.Support;

internal abstract class RepositoryStub
{
    public Exception? Failure { get; set; }
    public string? FailureMethod { get; set; }
    public List<string> Calls { get; } = [];
    public CancellationToken LastToken { get; private set; }

    protected Task<T> Complete<T>(string method, CancellationToken token, Func<T> result)
    {
        Calls.Add(method);
        LastToken = token;
        return (Failure == null || (FailureMethod != null && FailureMethod != method)) ? Task.FromResult(result()) : Task.FromException<T>(Failure);
    }

    protected Task Complete(string method, CancellationToken token)
    {
        Calls.Add(method);
        LastToken = token;
        return (Failure == null || (FailureMethod != null && FailureMethod != method)) ? Task.CompletedTask : Task.FromException(Failure);
    }
}

internal sealed class ProjectRepositoryStub : RepositoryStub, IProjectRepository
{
    public List<Project> Items { get; } = [];

    public Task<List<Project>> GetProjects(CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetProjects), cancellationToken, () => Items);
    }

    public Task<List<Project>> GetProjectsBySupervisorId(Guid supervisorId, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetProjectsBySupervisorId), cancellationToken, () => Items.Where(x => x.SupervisorId == supervisorId).ToList());
    }

    public Task<List<Project>> GetProjectsByTopicId(Guid topicId, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetProjectsByTopicId), cancellationToken, () => Items.Where(x => x.Topics.Any(t => t.Id == topicId)).ToList());
    }

    public Task<List<Project>> GetProjectsByStatus(ProjectStatus status, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetProjectsByStatus), cancellationToken, () => Items.Where(x => x.Status == status).ToList());
    }

    public Task<Project?> GetProjectById(Guid projectId, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetProjectById), cancellationToken, () => Items.SingleOrDefault(x => x.Id == projectId));
    }

    public Task<Project> CreateProject(Project project, IReadOnlyCollection<Guid> topicIds, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(CreateProject), cancellationToken, () => project);
    }

    public Task<Project> UpdateProject(Project project, IReadOnlyCollection<Guid> topicIds, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(UpdateProject), cancellationToken, () => project);
    }

    public Task<bool> DeleteProject(Guid projectId, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(DeleteProject), cancellationToken, () => Items.RemoveAll(x => x.Id == projectId) > 0);
    }
}

internal sealed class InquiryRepositoryStub : RepositoryStub, IInquiryRepository
{
    public List<Inquiry> Items { get; } = [];

    public Task<List<Inquiry>> GetInquiries(CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetInquiries), cancellationToken, () => Items);
    }

    public Task<List<Inquiry>> GetInquiriesByStudentId(Guid studentId, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetInquiriesByStudentId), cancellationToken, () => Items.Where(x => x.StudentId == studentId).ToList());
    }

    public Task<List<Inquiry>> GetInquiriesBySupervisorId(Guid supervisorId, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetInquiriesBySupervisorId), cancellationToken, () => Items.Where(x => x.SupervisorId == supervisorId).ToList());
    }

    public Task<List<Inquiry>> GetInquiriesByProjectId(Guid projectId, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetInquiriesByProjectId), cancellationToken, () => Items.Where(x => x.ProjectId == projectId).ToList());
    }

    public Task<Inquiry?> GetInquiryById(Guid inquiryId, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetInquiryById), cancellationToken, () => Items.SingleOrDefault(x => x.Id == inquiryId));
    }

    public Task<Inquiry> CreateInquiry(Inquiry inquiry, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(CreateInquiry), cancellationToken, () => inquiry);
    }

    public Task<Inquiry> UpdateInquiry(Inquiry inquiry, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(UpdateInquiry), cancellationToken, () => inquiry);
    }
}

internal sealed class StudentRepositoryStub : RepositoryStub, IStudentRepository
{
    public List<Student> Items { get; } = [];

    public Task<List<Student>> GetStudents(CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetStudents), cancellationToken, () => Items);
    }

    public Task<Student?> GetStudentById(Guid userId, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetStudentById), cancellationToken, () => Items.SingleOrDefault(x => x.UserId == userId));
    }

    public Task<Student> UpdateStudent(Student profile, IReadOnlyCollection<Guid> topicIds, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(UpdateStudent), cancellationToken, () => profile);
    }
}

internal sealed class SupervisorRepositoryStub : RepositoryStub, ISupervisorRepository
{
    public List<Supervisor> Items { get; } = [];

    public Task<List<Supervisor>> GetSupervisors(CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetSupervisors), cancellationToken, () => Items);
    }

    public Task<Supervisor?> GetSupervisorById(Guid userId, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetSupervisorById), cancellationToken, () => Items.SingleOrDefault(x => x.UserId == userId));
    }

    public Task<Supervisor> UpdateSupervisor(Supervisor profile, IReadOnlyCollection<Guid> topicIds, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(UpdateSupervisor), cancellationToken, () => profile);
    }
}

internal sealed class TopicRepositoryStub : RepositoryStub, ITopicRepository
{
    public List<Topic> Items { get; } = [];

    public Task<List<Topic>> GetTopics(CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetTopics), cancellationToken, () => Items);
    }

    public Task<List<Topic>> GetTopicsByIds(IReadOnlyCollection<Guid> topicIds, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetTopicsByIds), cancellationToken, () => Items.Where(x => topicIds.Contains(x.Id)).ToList());
    }
}

internal sealed class SavedProjectRepositoryStub : RepositoryStub, ISavedProjectRepository
{
    public List<Project> Items { get; } = [];

    public Task<List<Project>> GetSavedProjects(Guid studentId, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(GetSavedProjects), cancellationToken, () => Items);
    }

    public Task SaveProject(Guid studentId, Guid projectId, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(SaveProject), cancellationToken);
    }

    public Task RemoveSavedProject(Guid studentId, Guid projectId, CancellationToken cancellationToken = default)
    {
        return Complete(nameof(RemoveSavedProject), cancellationToken);
    }
}
