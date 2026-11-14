using uoProjects.Models;
using uoProjects.Services;
using uoProjects.Components;

namespace uoProjects.Admin;

public sealed class AdminData
{
    public static readonly TimeSpan StaleAfter = TimeSpan.FromDays(7);

    private readonly Dictionary<Guid, string> names;
    private readonly Dictionary<Guid, Project> projectsById;
    private readonly Dictionary<Guid, List<Topic>> projectTopics;
    private readonly Dictionary<Guid, int> saveCounts;
    private readonly ILookup<Guid, Inquiry> byStudent;
    private readonly ILookup<Guid, Inquiry> bySupervisor;
    private readonly ILookup<Guid?, Inquiry> byProject;
    private readonly ILookup<Guid, Project> projectsBySupervisor;

    internal AdminData(List<Student> students, List<Supervisor> supervisors, List<Project> projects, List<Inquiry> inquiries,
        List<Topic> topics, Dictionary<Guid, List<Topic>> projectTopics, Dictionary<Guid, int> saveCounts)
    {
        Students = [.. students.OrderBy(s => s.DisplayName)];
        Supervisors = [.. supervisors.OrderBy(s => s.DisplayName)];
        Projects = [.. projects.OrderBy(p => p.Title)];
        Inquiries = [.. inquiries.OrderByDescending(i => i.CreatedAt)];
        Topics = [.. topics.OrderBy(t => TopicPalette.OrderOf(t.Name))];
        this.projectTopics = projectTopics;
        this.saveCounts = saveCounts;

        names = students.ToDictionary(s => s.UserId, s => s.DisplayName);
        foreach (var supervisor in supervisors)
        {
            names[supervisor.UserId] = supervisor.DisplayName;
        }
        projectsById = projects.ToDictionary(p => p.Id);
        byStudent = Inquiries.ToLookup(i => i.StudentId);
        bySupervisor = Inquiries.ToLookup(i => i.SupervisorId);
        byProject = Inquiries.ToLookup(i => i.ProjectId);
        projectsBySupervisor = Projects.ToLookup(p => p.SupervisorId);
    }

    public List<Student> Students { get; }
    public List<Supervisor> Supervisors { get; }
    public List<Project> Projects { get; }
    public List<Inquiry> Inquiries { get; }
    public List<Topic> Topics { get; }

    public static async Task<AdminData?> LoadAsync(StudentsService studentsService, SupervisorsService supervisorsService,
        ProjectsService projectsService, InquiriesService inquiriesService, TopicsService topicsService, SavedProjectsService savedProjectsService)
    {
        var students = studentsService.ListAsync();
        var supervisors = supervisorsService.ListAsync();
        var projects = projectsService.GetProjectsWithTopicsAsync();
        var inquiries = inquiriesService.ListAsync();
        var saveCounts = savedProjectsService.CountsAsync();
        var topicsRequest = topicsService.ListAsync();

        await Task.WhenAll(students, supervisors, projects, inquiries, saveCounts, topicsRequest);
        if (students.Result is null || supervisors.Result is null || projects.Result is null ||
            inquiries.Result is null || saveCounts.Result is null || topicsRequest.Result is not { } topics)
        {
            return null;
        }

        var projectTopics = projects.Result.ToDictionary(project => project.Id,
            project => (project.Topics ?? []).OrderBy(topic => TopicPalette.OrderOf(topic.Name)).ToList());

        return new AdminData(students.Result, supervisors.Result, projects.Result, inquiries.Result, topics, projectTopics, saveCounts.Result);
    }

    public string NameOf(Guid userId)
    {
        return names.GetValueOrDefault(userId, "Unknown");
    }

    public Student? Student(Guid userId)
    {
        return Students.FirstOrDefault(s => s.UserId == userId);
    }

    public Supervisor? Supervisor(Guid userId)
    {
        return Supervisors.FirstOrDefault(s => s.UserId == userId);
    }

    public Project? Project(Guid? projectId)
    {
        return projectId is { } id ? projectsById.GetValueOrDefault(id) : null;
    }

    public IReadOnlyList<Topic> TopicsOf(Guid projectId)
    {
        return projectTopics.GetValueOrDefault(projectId) ?? [];
    }

    public UpdateProject UpdateFor(Project project)
    {
        return new(project.Title, project.Description, project.ExternalUrl, project.Status, [.. TopicsOf(project.Id).Select(t => t.Id)]);
    }

    public int SavesOf(Guid projectId)
    {
        return saveCounts.GetValueOrDefault(projectId);
    }

    public IEnumerable<Inquiry> InquiriesFrom(Guid studentId)
    {
        return byStudent[studentId];
    }

    public IEnumerable<Inquiry> InquiriesTo(Guid supervisorId)
    {
        return bySupervisor[supervisorId];
    }

    public IEnumerable<Inquiry> InquiriesFor(Guid projectId)
    {
        return byProject[projectId];
    }

    public IEnumerable<Project> ProjectsOf(Guid supervisorId)
    {
        return projectsBySupervisor[supervisorId];
    }

    public InquiryStatus? FurthestStage(Guid studentId)
    {
        return byStudent[studentId].Select(i => (InquiryStatus?)i.Status).OrderByDescending(s => s is InquiryStatus.Matched ? 2 : s is InquiryStatus.Sent ? 1 : 0).FirstOrDefault();
    }

    public DateTime? LastActivity(Guid studentId)
    {
        return byStudent[studentId].Max(i => (DateTime?)i.UpdatedAt);
    }

    public double? MatchRate(Guid supervisorId)
    {
        var received = bySupervisor[supervisorId].ToList();
        return received.Count == 0 ? null : (double)received.Count(i => i.Status == InquiryStatus.Matched) / received.Count;
    }

    public static bool IsOpen(Inquiry inquiry)
    {
        return inquiry.Status == InquiryStatus.Sent;
    }

    public static bool IsStale(Inquiry inquiry)
    {
        return IsOpen(inquiry) && Age(inquiry.CreatedAt) > StaleAfter;
    }

    public IEnumerable<Inquiry> Stale => Inquiries.Where(IsStale);

    public static TimeSpan Age(DateTime time)
    {
        return DateTime.UtcNow - time.ToUniversalTime();
    }
}
