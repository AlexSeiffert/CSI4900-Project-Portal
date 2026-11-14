using Microsoft.AspNetCore.Components;
using uoProjects.Admin;
using uoProjects.Models;

namespace uoProjects.Admin.Insights;

public partial class ProjectInsights
{
private const int Top = 10;

    [Parameter, EditorRequired] public AdminData Data { get; set; } = default!;
    [Parameter, EditorRequired] public string BasePath { get; set; } = "";

    private object popular = new();
    private object ring = new();
    private List<(string Label, int Count, string Color)> statuses = [];

    protected override void OnParametersSet()
    {
        var top = Data.Projects
            .OrderByDescending(p => Data.InquiriesFor(p.Id).Count()).ThenByDescending(p => Data.SavesOf(p.Id))
            .Take(Top).ToList();
        popular = new
        {
            categories = top.Select(p => InsightMath.Clip(p.Title)),
            horizontal = true,
            links = top.Select(p => $"{BasePath}/projects/{p.Id}"),
            series = new object[]
            {
                new { name = "Inquiries", color = "mono-5", data = top.Select(p => Data.InquiriesFor(p.Id).Count()) },
                new { name = "Saves", color = "stage-1", type = "line", data = top.Select(p => Data.SavesOf(p.Id)) },
            },
        };
        statuses = [.. Enum.GetValues<ProjectStatus>().Select(status =>
            (Display.Project(status).Label, Data.Projects.Count(p => p.Status == status), Display.Project(status).Color))];
        ring = new
        {
            caption = "projects",
            segments = statuses.Select(s => new { name = s.Label, value = s.Count, color = s.Color }),
        };
    }
}
