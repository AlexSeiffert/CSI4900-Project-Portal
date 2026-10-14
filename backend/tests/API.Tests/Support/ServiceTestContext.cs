using API.Orchestrations;
using API.Models;
using API.Services;
using API.DTOs.Request;

namespace API.Tests.Support;

internal sealed class ServiceTestContext
{
    public Topic Topic { get; } = new() { Id = Guid.NewGuid(), Name = "Databases" };
    public Student Student { get; } = new() { UserId = Guid.NewGuid(), DisplayName = "Student", Program = "CS" };
    public Supervisor Supervisor { get; } = new() { UserId = Guid.NewGuid(), DisplayName = "Supervisor", ContactEmail = "supervisor@example.test" };
    public Project Project { get; }
    public Inquiry Inquiry { get; }
    public ProjectRepositoryStub Projects { get; } = new();
    public InquiryRepositoryStub Inquiries { get; } = new();
    public StudentRepositoryStub Students { get; } = new();
    public SupervisorRepositoryStub Supervisors { get; } = new();
    public TopicRepositoryStub Topics { get; } = new();
    public SavedProjectRepositoryStub SavedProjects { get; } = new();
    public ProjectService ProjectService => new(Projects);
    public InquiryService InquiryService => new(Inquiries);
    public StudentService StudentService => new(Students);
    public SupervisorService SupervisorService => new(Supervisors);
    public TopicService TopicService => new(Topics);
    public SavedProjectService SavedProjectService => new(SavedProjects);

    public ProjectOrchestration ProjectOrchestration => new(ProjectService, SupervisorService, TopicService);

    public StudentOrchestration StudentOrchestration => new(StudentService, TopicService);

    public SupervisorOrchestration SupervisorOrchestration => new(SupervisorService, TopicService);

    public InquiryOrchestration InquiryOrchestration => new(InquiryService, StudentService, SupervisorService, ProjectService);

    public SavedProjectOrchestration SavedProjectOrchestration => new(SavedProjectService, StudentService, ProjectService);

    public ServiceTestContext()
    {
        Student.TopicInterests.Add(Topic);
        Supervisor.TopicInterests.Add(Topic);
        Project = new() { Id = Guid.NewGuid(), SupervisorId = Supervisor.UserId, Title = "Portal", Description = "Research", Topics = [Topic] };
        Inquiry = new() { Id = Guid.NewGuid(), StudentId = Student.UserId, SupervisorId = Supervisor.UserId, ProjectId = Project.Id, Message = "Original message" };
        Projects.Items.Add(Project);
        Inquiries.Items.Add(Inquiry);
        Students.Items.Add(Student);
        Supervisors.Items.Add(Supervisor);
        Topics.Items.Add(Topic);
        SavedProjects.Items.Add(Project);
    }

    public CreateProject NewProject() => new() { SupervisorId = Supervisor.UserId, Title = "  New portal  ", Description = "  Research  ", TopicIds = [Topic.Id, Topic.Id] };
    public UpdateProject ProjectUpdate() => new() { Title = "  Updated  ", Description = "  Description  ", Status = ProjectStatus.Archived, TopicIds = [Topic.Id] };
    public CreateInquiry NewInquiry() => new() { StudentId = Student.UserId, SupervisorId = Supervisor.UserId, ProjectId = Project.Id, Message = "Interested" };
    public UpdateStudent StudentUpdate() => new() { DisplayName = "Updated", Program = "Engineering", TopicIds = [Topic.Id] };
    public UpdateSupervisor SupervisorUpdate() => new() { DisplayName = "Updated", ContactEmail = "updated@example.test", AboutMe = "Research", ExternalUrl = "https://example.test", AcceptingInquiries = false, TopicIds = [Topic.Id] };
}
