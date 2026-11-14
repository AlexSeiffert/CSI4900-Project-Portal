using Microsoft.AspNetCore.Components;
using uoProjects.Admin;
using uoProjects.Auth;
using uoProjects.Models;
using uoProjects.Services;

namespace uoProjects.Pages;

public partial class AdminHome
{
    [Inject] private StudentsService Students { get; set; } = default!;
    [Inject] private SupervisorsService Supervisors { get; set; } = default!;
    [Inject] private ProjectsService Projects { get; set; } = default!;
    [Inject] private InquiriesService Inquiries { get; set; } = default!;
    [Inject] private TopicsService Topics { get; set; } = default!;
    [Inject] private SavedProjectsService SavedProjects { get; set; } = default!;
    [Inject] private AuthSession Auth { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    [Parameter] public string? Tab { get; set; }
    [Parameter] public string? ItemId { get; set; }

    private string current = "insights";
    private AdminData? data;
    private string? error;
    private bool loading;
    private AdminActions actions = default!;

    private bool IsProjectsList => current == "projects" && ItemId is null;

    protected override void OnInitialized() => actions = new AdminActions(ReloadAsync);

    protected override async Task OnParametersSetAsync()
    {
        current = AdminShell.Normalize(Tab);
        if (!IsProjectsList && data is null && error is null && (await Auth.GetUserAsync())?.Role == "admin")
        {
            await ReloadAsync();
        }
    }

    private void InvalidateDashboard()
    {
        data = null;
        error = null;
    }

    private async Task ReloadAsync()
    {
        if (loading)
        {
            return;
        }

        loading = true;
        error = null;
        StateHasChanged();
        try
        {
            data = await AdminData.LoadAsync(Students, Supervisors, Projects, Inquiries, Topics, SavedProjects);
            error = data is null ? "Could not refresh the dashboard." : null;
        }
        finally
        {
            loading = false;
            StateHasChanged();
        }
    }

    private async Task SignOutAsync()
    {
        await Auth.SignOutAsync();
        Nav.NavigateTo("/login", replace: true);
    }
}
