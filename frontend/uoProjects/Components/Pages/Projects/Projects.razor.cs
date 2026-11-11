using Microsoft.AspNetCore.Components;
using uoProjects.Models;

namespace uoProjects.Components.Pages.Projects;

public partial class Projects : IDisposable
{
    [PersistentState]
    public List<Project>? ProjectList { get; set; }

    [PersistentState]
    public bool HasLoaded { get; set; }

    private bool IsLoading { get; set; } = true;
    private bool restored;
    private readonly CancellationTokenSource cancellation = new();

    protected override async Task OnInitializedAsync()
    {
        restored = HasLoaded;
        if (!HasLoaded)
        {
            await LoadDataAsync();
        }
        else
        {
            IsLoading = false;
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        // Restore a prerendered failure without repeating the request or its log entry.
        if (firstRender && restored && ProjectList is null)
        {
            Notifications.Error("An error occurred when fetching projects. Please try again.");
        }
    }

    private async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            ProjectList = await ProjectsService.GetProjectsAsync(cancellation.Token);
            HasLoaded = true;
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            // The page was disposed while loading.
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void Dispose()
    {
        cancellation.Cancel();
        cancellation.Dispose();
    }
}
