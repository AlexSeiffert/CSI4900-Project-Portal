using API.DTOs.Request;
using API.Orchestrations;

namespace API.Tests.Support;

internal sealed class OrchestrationTestContext
{
    public ProjectServiceStub Projects { get; } = new();
    public InquiryServiceStub Inquiries { get; } = new();
    public StudentServiceStub Students { get; } = new();
    public SupervisorServiceStub Supervisors { get; } = new();
    public TopicServiceStub Topics { get; } = new();
    public SavedProjectServiceStub SavedProjects { get; } = new();
    public List<string> Calls { get; } = [];
    public ProjectOrchestration ProjectOrchestration => new(Projects, Supervisors, Topics);
    public InquiryOrchestration InquiryOrchestration => new(Inquiries, Students, Supervisors, Projects);
    public StudentOrchestration StudentOrchestration => new(Students, Topics);
    public SupervisorOrchestration SupervisorOrchestration => new(Supervisors, Topics);
    public SavedProjectOrchestration SavedProjectOrchestration => new(SavedProjects, Students, Projects);

    public OrchestrationTestContext()
    {
        Projects.OnCall = method => Calls.Add("Projects." + method);
        Inquiries.OnCall = method => Calls.Add("Inquiries." + method);
        Students.OnCall = method => Calls.Add("Students." + method);
        Supervisors.OnCall = method => Calls.Add("Supervisors." + method);
        Topics.OnCall = method => Calls.Add("Topics." + method);
        SavedProjects.OnCall = method => Calls.Add("SavedProjects." + method);
        SavedProjects.Items.Add(Projects.Item);
    }

    public CreateProject NewProject() => new() { SupervisorId = Supervisors.Item.UserId, Title = "Portal", Description = "Research", TopicIds = [Topics.Item.Id] };
    public UpdateProject ProjectUpdate() => new() { Title = "Updated", Description = "Research", TopicIds = [Topics.Item.Id] };
    public UpdateStudent StudentUpdate() => new() { DisplayName = "Updated", TopicIds = [Topics.Item.Id] };
    public UpdateSupervisor SupervisorUpdate() => new() { DisplayName = "Updated", ContactEmail = "updated@example.test", TopicIds = [Topics.Item.Id] };
    public CreateInquiry NewInquiry() => new() { StudentId = Students.Item.UserId, SupervisorId = Supervisors.Item.UserId, ProjectId = Projects.Item.Id, Message = "Interested" };
}
