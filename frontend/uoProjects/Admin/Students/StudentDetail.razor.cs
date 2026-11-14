using Microsoft.AspNetCore.Components;
using uoProjects.Models;
using uoProjects.Services;

namespace uoProjects.Admin;

public partial class StudentDetail
{
    [Inject] private StudentsService Students { get; set; } = default!;
    [Inject] private SavedProjectsService SavedProjects { get; set; } = default!;

    [CascadingParameter] public AdminActions Actions { get; set; } = default!;
    [Parameter, EditorRequired] public AdminData Data { get; set; } = default!;
    [Parameter, EditorRequired] public string BasePath { get; set; } = "";
    [Parameter, EditorRequired] public Guid Id { get; set; }

    private Student? student;
    private List<Project>? savedProjects;
    private Guid? loadedId;
    private bool loaded;
    private string displayName = "";
    private HashSet<Guid> topics = [];
    private bool busy;
    private bool saved;
    private string? error;

    protected override async Task OnParametersSetAsync()
    {
        if (loadedId == Id)
        {
            student = student is not null && Data.Student(Id) is { } fresh ? fresh with { TopicInterests = student?.TopicInterests } : null;
            return;
        }
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        loadedId = Id;
        loaded = false;
        saved = false;
        error = null;
        student = await Actions.GetAsync(() => Students.GetAsync(Id), Data.Student(Id));
        savedProjects = await Actions.GetAsync(() => SavedProjects.ListAsync(Id), new List<Project>());
        loaded = true;
        if (student is not null)
        {
            displayName = student.DisplayName;
            topics = [.. (student.TopicInterests ?? []).Select(t => t.Id)];
        }
    }

    private async Task SaveAsync()
    {
        busy = true;
        saved = false;
        var body = new UpdateStudent(displayName.Trim(), [.. topics], student!.Program);
        error = await Actions.SaveAsync(() => Students.UpdateAsync(Id, body));
        saved = error is null;
        busy = false;
    }
}
