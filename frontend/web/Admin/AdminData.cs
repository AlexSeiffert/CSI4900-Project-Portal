using ProjectPortal.Web.Api;
using ProjectPortal.Web.Components;

namespace ProjectPortal.Web.Admin;

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

    public static async Task<AdminData> LoadAsync(ApiClient api)
    {
        var students = api.GetAsync<List<Student>>("students");
        var supervisors = api.GetAsync<List<Supervisor>>("supervisors");
        var projects = api.GetAsync<List<Project>>("projects");
        var inquiries = api.GetAsync<List<Inquiry>>("inquiries");
        var saveCounts = api.GetAsync<Dictionary<Guid, int>>("saved-projects/counts");
        var topics = await api.GetAsync<List<Topic>>("topics");

        var tagged = await Task.WhenAll(topics.Select(async topic => (topic, projects: await api.GetAsync<List<Project>>($"projects/topics/{topic.Id}"))));
        var projectTopics = tagged
            .SelectMany(t => t.projects.Select(p => (p.Id, t.topic)))
            .GroupBy(x => x.Id)
            .ToDictionary(g => g.Key, g => g.Select(x => x.topic).OrderBy(t => TopicPalette.OrderOf(t.Name)).ToList());

        await Task.WhenAll(students, supervisors, projects, inquiries, saveCounts);
        return new AdminData(students.Result, supervisors.Result, projects.Result, inquiries.Result, topics, projectTopics, saveCounts.Result);
    }

    public string NameOf(Guid userId) => names.GetValueOrDefault(userId, "Unknown");

    public Student? Student(Guid userId) => Students.FirstOrDefault(s => s.UserId == userId);

    public Supervisor? Supervisor(Guid userId) => Supervisors.FirstOrDefault(s => s.UserId == userId);

    public Project? Project(Guid? projectId) => projectId is { } id ? projectsById.GetValueOrDefault(id) : null;

    public IReadOnlyList<Topic> TopicsOf(Guid projectId) => projectTopics.GetValueOrDefault(projectId) ?? [];

    public UpdateProject UpdateFor(Project project) =>
        new(project.Title, project.Description, project.ExternalUrl, project.Status, [.. TopicsOf(project.Id).Select(t => t.Id)]);

    public int SavesOf(Guid projectId) => saveCounts.GetValueOrDefault(projectId);

    public IEnumerable<Inquiry> InquiriesFrom(Guid studentId) => byStudent[studentId];

    public IEnumerable<Inquiry> InquiriesTo(Guid supervisorId) => bySupervisor[supervisorId];

    public IEnumerable<Inquiry> InquiriesFor(Guid projectId) => byProject[projectId];

    public IEnumerable<Project> ProjectsOf(Guid supervisorId) => projectsBySupervisor[supervisorId];

    public InquiryStatus? FurthestStage(Guid studentId) =>
        byStudent[studentId].Select(i => (InquiryStatus?)i.Status).OrderByDescending(s => s is InquiryStatus.Matched ? 2 : s is InquiryStatus.Sent ? 1 : 0).FirstOrDefault();

    public DateTime? LastActivity(Guid studentId) => byStudent[studentId].Max(i => (DateTime?)i.UpdatedAt);

    public double? MatchRate(Guid supervisorId)
    {
        var received = bySupervisor[supervisorId].ToList();
        return received.Count == 0 ? null : (double)received.Count(i => i.Status == InquiryStatus.Matched) / received.Count;
    }

    public static bool IsOpen(Inquiry inquiry) => inquiry.Status == InquiryStatus.Sent;

    public static bool IsStale(Inquiry inquiry) => IsOpen(inquiry) && Age(inquiry.CreatedAt) > StaleAfter;

    public IEnumerable<Inquiry> Stale => Inquiries.Where(IsStale);

    public static TimeSpan Age(DateTime time) => DateTime.UtcNow - time.ToUniversalTime();
}
