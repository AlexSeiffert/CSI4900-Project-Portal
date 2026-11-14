using Microsoft.AspNetCore.Components;
using uoProjects.Models;
using uoProjects.Services;

namespace uoProjects.Admin;

public partial class SupervisorDetail
{
    [Inject] private SupervisorsService Supervisors { get; set; } = default!;

    [CascadingParameter] public AdminActions Actions { get; set; } = default!;
    [Parameter, EditorRequired] public AdminData Data { get; set; } = default!;
    [Parameter, EditorRequired] public string BasePath { get; set; } = "";
    [Parameter, EditorRequired] public Guid Id { get; set; }

    private Supervisor? supervisor;
    private Guid? loadedId;
    private bool loaded;
    private Form form = new();
    private bool busy;
    private bool saved;
    private string? error;

    protected override async Task OnParametersSetAsync()
    {
        if (loadedId == Id)
        {
            if (supervisor is not null)
            {
                supervisor = Data.Supervisor(Id);
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
        supervisor = await Actions.GetAsync(() => Supervisors.GetAsync(Id), Data.Supervisor(Id));
        loaded = true;
        if (supervisor is not null)
        {
            form = new Form
            {
                Name = supervisor.DisplayName,
                Email = supervisor.ContactEmail,
                Url = supervisor.ExternalUrl ?? "",
                About = supervisor.AboutMe ?? "",
                Accepting = supervisor.AcceptingInquiries,
                Topics = [.. (supervisor.TopicInterests ?? []).Select(t => t.Id)],
            };
        }
    }

    private async Task SaveAsync()
    {
        busy = true;
        saved = false;
        var body = new UpdateSupervisor(form.Name.Trim(), form.Email.Trim(), Blank(form.About), Blank(form.Url), form.Accepting, [.. form.Topics]);
        error = await Actions.SaveAsync(() => Supervisors.UpdateAsync(Id, body));
        saved = error is null;
        busy = false;
    }

    private static string? Blank(string text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    private sealed class Form
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Url { get; set; } = "";
        public string About { get; set; } = "";
        public bool Accepting { get; set; }
        public HashSet<Guid> Topics { get; set; } = [];
    }
}
