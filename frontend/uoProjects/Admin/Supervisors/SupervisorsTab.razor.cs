using System.Web;
using Microsoft.AspNetCore.Components;
using uoProjects.Components;
using uoProjects.Models;
using uoProjects.Services;

namespace uoProjects.Admin;

public partial class SupervisorsTab
{
    [Inject] private SupervisorsService Supervisors { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    [CascadingParameter] public AdminActions Actions { get; set; } = default!;
    [Parameter, EditorRequired] public AdminData Data { get; set; } = default!;
    [Parameter, EditorRequired] public string BasePath { get; set; } = "";

    private string query = "";
    private string filter = "all";
    private List<FilterChips.Option> filters = [];
    private Guid? busy;
    private string? error;

    private int OpenProjects(Supervisor s) => Data.ProjectsOf(s.UserId).Count(p => p.Status != ProjectStatus.Archived);
    private string MatchRateText(Supervisor s) => Data.MatchRate(s.UserId) is { } r ? $"{Math.Round(r * 100)}%" : "";
    private int Stale(Supervisor s) => Data.InquiriesTo(s.UserId).Count(AdminData.IsStale);

    private bool Keeps(string key, Supervisor s) => key switch
    {
        "accepting" => s.AcceptingInquiries,
        "paused" => !s.AcceptingInquiries,
        "stale" => Stale(s) > 0,
        _ => true,
    };

    private List<Supervisor> shown => Data.Supervisors
        .Where(s => Keeps(filter, s))
        .Where(s => s.DisplayName.Contains(query.Trim(), StringComparison.OrdinalIgnoreCase) || s.ContactEmail.Contains(query.Trim(), StringComparison.OrdinalIgnoreCase))
        .ToList();

    protected override void OnInitialized()
    {
        var from = HttpUtility.ParseQueryString(new Uri(Nav.Uri).Query)["filter"];
        if (from is not null && from is "accepting" or "paused" or "stale")
        {
            filter = from;
        }
    }

    protected override void OnParametersSet()
    {
        int Count(string key) => Data.Supervisors.Count(s => Keeps(key, s));
        filters =
        [
            new("all", "All", Count("all")),
            new("accepting", "Accepting", Count("accepting")),
            new("paused", "Paused", Count("paused")),
            new("stale", "Open 7+ days", Count("stale")),
        ];
    }

    private async Task SetAcceptingAsync(Supervisor supervisor, bool accepting)
    {
        busy = supervisor.UserId;
        error = await Actions.SaveAsync(async () =>
        {
            var full = await Supervisors.GetAsync(supervisor.UserId);
            if (full is null)
            {
                return null;
            }
            return await Supervisors.UpdateAsync(supervisor.UserId, new UpdateSupervisor(
                full.DisplayName, full.ContactEmail, full.AboutMe, full.ExternalUrl, accepting, [.. (full.TopicInterests ?? []).Select(t => t.Id)]));
        });
        busy = null;
    }
}
