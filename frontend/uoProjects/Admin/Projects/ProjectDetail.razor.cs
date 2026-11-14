using System.Web;
using Microsoft.AspNetCore.Components;
using uoProjects.Models;
using uoProjects.Services;

namespace uoProjects.Admin;

public partial class ProjectDetail
{
    [Inject] private ProjectsService Projects { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    [CascadingParameter] public AdminActions Actions { get; set; } = default!;
    [Parameter, EditorRequired] public AdminData Data { get; set; } = default!;
    [Parameter, EditorRequired] public string BasePath { get; set; } = "";
    [Parameter] public Guid? Id { get; set; }

    private bool IsNew => Id is null;

    private Project? project;
    private Guid? loadedId;
    private bool loaded;
    private Form form = new();
    private bool busy;
    private bool saved;
    private string? error;
    private bool confirmingDelete;

    protected override async Task OnParametersSetAsync()
    {
        if (IsNew)
        {
            if (loadedId is not null || !loaded)
            {
                var from = HttpUtility.ParseQueryString(new Uri(Nav.Uri).Query)["supervisor"];
                form = new Form { SupervisorId = Guid.TryParse(from, out var s) ? s.ToString() : "" };
                loadedId = null;
                loaded = true;
            }
            return;
        }
        if (loadedId == Id)
        {
            if (project is not null)
            {
                project = Data.Project(Id) ?? project;
            }
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
        confirmingDelete = false;
        project = await Actions.GetAsync(() => Projects.GetProjectAsync(Id!.Value), Data.Project(Id));
        loaded = true;
        if (project is not null)
        {
            form = new Form
            {
                SupervisorId = project.SupervisorId.ToString(),
                Title = project.Title,
                Description = project.Description,
                Url = project.ExternalUrl ?? "",
                Status = project.Status,
                Topics = [.. (project.Topics ?? [.. Data.TopicsOf(project.Id)]).Select(t => t.Id)],
            };
        }
    }

    private void SetStatus(string value) => form.Status = Enum.Parse<ProjectStatus>(value);

    private async Task SaveAsync()
    {
        if (busy || !Actions.CanSave)
        {
            return;
        }
        if (!Guid.TryParse(form.SupervisorId, out var supervisorId) || !Data.Supervisors.Any(s => s.UserId == supervisorId))
        {
            saved = false;
            error = "Choose a supervisor.";
            return;
        }

        error = null;
        busy = true;
        saved = false;
        var body = new UpdateProject(form.Title.Trim(), form.Description.Trim(), string.IsNullOrWhiteSpace(form.Url) ? null : form.Url.Trim(),
            form.Status, [.. form.Topics], supervisorId);
        if (IsNew)
        {
            Project? created = null;
            error = await Actions.SaveAsync(async () => created = await Projects.CreateAsync(body));
            busy = false;
            if (created is not null)
            {
                Nav.NavigateTo($"{BasePath}/projects/{created.Id}");
            }
            return;
        }
        error = await Actions.SaveAsync(async () =>
        {
            var updated = await Projects.UpdateAsync(Id!.Value, body);
            if (updated is not null)
            {
                project = updated;
            }
            return updated;
        });
        saved = error is null;
        busy = false;
    }

    private async Task DeleteAsync()
    {
        busy = true;
        error = await Actions.SaveAsync(() => Projects.DeleteAsync(Id!.Value));
        busy = false;
        if (error is null)
        {
            Nav.NavigateTo($"{BasePath}/projects");
        }
    }

    private sealed class Form
    {
        public string SupervisorId { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Url { get; set; } = "";
        public ProjectStatus Status { get; set; } = ProjectStatus.Available;
        public HashSet<Guid> Topics { get; set; } = [];
    }
}
