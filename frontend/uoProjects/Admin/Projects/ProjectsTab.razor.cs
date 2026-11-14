using Microsoft.AspNetCore.Components;
using System.Web;
using uoProjects.Components;
using uoProjects.Models;
using uoProjects.Services;

namespace uoProjects.Admin;

public partial class ProjectsTab : IDisposable
{
    [Inject] private ProjectsService Projects { get; set; } = default!;
    [Inject] private TopicsService Topics { get; set; } = default!;
    [Inject] private SupervisorsService Supervisors { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    [Parameter] public AdminData? Data { get; set; }
    [Parameter, EditorRequired] public string BasePath { get; set; } = "";
    [Parameter] public EventCallback OnChanged { get; set; }

    private ProjectPageQuery request = new();
    private CancellationTokenSource? pending;
    private readonly CancellationTokenSource lifetime = new();
    private List<ProjectPageRow> rows = [];
    private List<FilterChips.Option> filters = [];
    private List<Topic>? topics;
    private List<Supervisor>? supervisors;
    private Task? optionsLoad;
    private bool optionsFailed;
    private (Guid Id, bool Status)? editor;
    private readonly Dictionary<ProjectPageQuery, (ProjectPage Page, DateTimeOffset Expires)> recentPages = [];
    private ProjectPageQuery? displayedRequest;
    private bool TableUnavailable => displayedRequest != request || loading || loadFailed || busy.HasValue;
    internal static readonly (string Value, string Label)[] StatusOptions = Enum.GetValues<ProjectStatus>()
        .Select(status => (status.ToString(), Display.Project(status).Label)).ToArray();
    private int total;
    private int allTotal;
    private bool loading = true;
    private bool loadFailed;
    private Guid? busy;
    private string? error;
    private string query = "";
    private string topic = "";
    private string filter = "all";

    private int FirstRow => ((displayedRequest?.Page ?? 1) - 1) * request.PageSize + 1;
    private int LastRow => FirstRow + rows.Count - 1;
    private IEnumerable<(string, string)> TopicOptions =>
        (topics ?? []).OrderBy(t => TopicPalette.OrderOf(t.Name)).Select(t => (t.Id.ToString(), t.Name)).Prepend(("", "Any"));
    private (string, string)[] SupervisorOptions = [];

    protected override Task OnInitializedAsync()
    {
        var initial = HttpUtility.ParseQueryString(Nav.ToAbsoluteUri(Nav.Uri).Query);
        if (Enum.TryParse<ProjectStatus>(initial["filter"], true, out var status) && Enum.IsDefined(status))
        {
            request = request with { Status = status };
            filter = status.ToString();
        }

        if (Data is { } preview)
        {
            topics = preview.Topics;
            supervisors = preview.Supervisors;
            SetSupervisorOptions();
        }

        optionsLoad = LoadOptionsAsync();
        return RetryAsync();
    }

    private async Task LoadOptionsAsync()
    {
        var token = lifetime.Token;
        optionsFailed = false;
        try
        {
            var topicsTask = topics is null ? Topics.ListAsync(token) : Task.FromResult<List<Topic>?>(topics);
            var supervisorsTask = supervisors is null ? Supervisors.ListAsync(token) : Task.FromResult<List<Supervisor>?>(supervisors);
            await Task.WhenAll(topicsTask, supervisorsTask);
            token.ThrowIfCancellationRequested();
            topics = await topicsTask;
            supervisors = await supervisorsTask;
            SetSupervisorOptions();
            optionsFailed = topics is null || supervisors is null;
            await InvokeAsync(StateHasChanged);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
    }

    private Task RetryOptionsAsync()
    {
        return optionsLoad is { IsCompleted: false } ? optionsLoad : optionsLoad = LoadOptionsAsync();
    }

    private void SetSupervisorOptions()
    {
        SupervisorOptions = (supervisors ?? []).OrderBy(s => s.DisplayName)
            .Select(s => (s.UserId.ToString(), s.DisplayName)).ToArray();
    }

    private Task RetryAsync() => LoadAsync(request);
    private Task PreviousAsync() => LoadAsync(request with { Page = request.Page - 1 });
    private Task NextAsync() => LoadAsync(request with { Page = request.Page + 1 });

    private Task SetStatusFilterAsync(string value)
    {
        filter = value;
        return LoadAsync(request with { Page = 1, Status = Enum.TryParse<ProjectStatus>(value, out var status) ? status : null });
    }

    private Task SetTopicAsync(string value)
    {
        topic = value;
        return LoadAsync(request with { Page = 1, TopicId = Guid.TryParse(value, out var id) ? id : null });
    }

    private Task SearchAsync(string value)
    {
        query = value ?? "";
        return LoadAsync(request with { Page = 1, Search = query.Trim() }, debounce: true);
    }

    private Task SetSortAsync((string Column, bool Descending) sort) =>
        LoadAsync(request with { Page = 1, Sort = Enum.Parse<ProjectSort>(sort.Column), Descending = sort.Descending });

    private async Task LoadAsync(ProjectPageQuery next, bool debounce = false)
    {
        pending?.Cancel();
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token);
        pending = cancellation;
        var token = cancellation.Token;
        request = next;
        editor = null;
        loading = true;
        loadFailed = false;
        try
        {
            if (!recentPages.TryGetValue(next, out var cached) || cached.Expires <= DateTimeOffset.UtcNow)
            {
                if (debounce)
                {
                    await Task.Delay(150, token);
                }
                var result = await ReadPageAsync(next, token);
                token.ThrowIfCancellationRequested();
                if (result is not null)
                {
                    if (recentPages.Count >= 20)
                    {
                        recentPages.Remove(recentPages.MinBy(entry => entry.Value.Expires).Key);
                    }
                    cached = (result, DateTimeOffset.UtcNow.AddSeconds(30));
                    recentPages[next] = cached;
                }
            }

            var page = cached.Expires > DateTimeOffset.UtcNow ? cached.Page : null;
            token.ThrowIfCancellationRequested();
            if (page is not null && page.Items.Count == 0 && next.Page > 1)
            {
                request = next with { Page = (Math.Max(1, page.Total) - 1) / next.PageSize + 1 };
                page = await ReadPageAsync(request, token);
                token.ThrowIfCancellationRequested();
            }
            if (page is null)
            {
                loadFailed = true;
                return;
            }

            displayedRequest = request;
            rows = page.Items;
            total = page.Total;
            allTotal = page.StatusCounts.Values.Sum();
            filters = [new("all", "All", allTotal), .. Enum.GetValues<ProjectStatus>().Select(status =>
                new FilterChips.Option(status.ToString(), Display.Project(status).Label, page.StatusCounts.GetValueOrDefault(status)))];
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
        finally
        {
            if (pending == cancellation)
            {
                pending = null;
                loading = false;
            }
        }
    }

    private Task<ProjectPage?> ReadPageAsync(ProjectPageQuery selection, CancellationToken token)
    {
        if (Data is not { } preview)
        {
            return Projects.GetPageAsync(selection, token);
        }

        var matches = preview.Projects.Where(project =>
            (selection.Status is null || project.Status == selection.Status) &&
            (selection.TopicId is null || preview.TopicsOf(project.Id).Any(t => t.Id == selection.TopicId)) &&
            (project.Title.Contains(selection.Search, StringComparison.OrdinalIgnoreCase) ||
                preview.NameOf(project.SupervisorId).Contains(selection.Search, StringComparison.OrdinalIgnoreCase)))
            .Select(project => new ProjectPageRow(project with { Topics = [.. preview.TopicsOf(project.Id)] },
                preview.NameOf(project.SupervisorId), preview.InquiriesFor(project.Id).Count(), preview.SavesOf(project.Id)))
            .ToList();

        Func<ProjectPageRow, IComparable> key = selection.Sort switch
        {
            ProjectSort.Supervisor => row => row.SupervisorName.ToLowerInvariant(),
            ProjectSort.Status => row => row.Project.Status,
            ProjectSort.Inquiries => row => row.InquiryCount,
            ProjectSort.Saves => row => row.SaveCount,
            ProjectSort.Updated => row => row.Project.UpdatedAt,
            _ => row => row.Project.Title.ToLowerInvariant()
        };
        var ordered = selection.Descending ? matches.OrderByDescending(key) : matches.OrderBy(key);
        var items = ordered.ThenBy(row => row.Project.Id)
            .Skip((selection.Page - 1) * selection.PageSize).Take(selection.PageSize).ToList();
        return Task.FromResult<ProjectPage?>(new(items, matches.Count,
            preview.Projects.GroupBy(project => project.Status).ToDictionary(group => group.Key, group => group.Count())));
    }

    private async Task<IEnumerable<ProjectPageRow>> ExportRowsAsync(string scope)
    {
        if (TableUnavailable)
        {
            throw new InvalidOperationException("The project list is unavailable.");
        }

        var token = lifetime.Token;
        var selection = request with { Page = 1, PageSize = 100 };
        if (scope == "all")
        {
            selection = selection with { Search = "", Status = null, TopicId = null };
        }

        var result = new List<ProjectPageRow>();
        try
        {
            for (var pageNumber = 1; ; pageNumber++)
            {
                var page = await ReadPageAsync(selection with { Page = pageNumber }, token)
                    ?? throw new InvalidOperationException("Could not load export rows.");
                result.AddRange(page.Items);
                if (page.Items.Count < selection.PageSize || result.Count >= page.Total)
                {
                    return result;
                }
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            throw new InvalidOperationException("The export was cancelled.");
        }
    }

    private void CloseEditor(bool open)
    {
        if (!open)
        {
            editor = null;
        }
    }

    private Task SetStatusAsync(ProjectPageRow row, string? value) =>
        Enum.TryParse<ProjectStatus>(value, out var status) && status != row.Project.Status
            ? SaveAsync(row.Project, status, null)
            : Task.CompletedTask;

    private Task MoveAsync(ProjectPageRow row, string? value) =>
        Guid.TryParse(value, out var id) && id != row.Project.SupervisorId
            ? SaveAsync(row.Project, row.Project.Status, id)
            : Task.CompletedTask;

    private async Task SaveAsync(Project project, ProjectStatus status, Guid? supervisor)
    {
        if (TableUnavailable || Data is not null)
        {
            return;
        }

        busy = project.Id;
        error = null;
        try
        {
            var update = new UpdateProject(project.Title, project.Description, project.ExternalUrl,
                status, [.. (project.Topics ?? []).Select(t => t.Id)], supervisor);
            if (await Projects.UpdateAsync(project.Id, update) is null)
            {
                error = "Changes could not be saved.";
                return;
            }

            recentPages.Clear();
            await OnChanged.InvokeAsync();
            if (!lifetime.IsCancellationRequested)
            {
                await RetryAsync();
            }
        }
        finally
        {
            busy = null;
        }
    }

    public void Dispose()
    {
        lifetime.Cancel();
        lifetime.Dispose();
    }
}
