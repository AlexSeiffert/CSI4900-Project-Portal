using Microsoft.AspNetCore.Components;
using uoProjects.Components;
using uoProjects.Models;

namespace uoProjects.Admin;

public partial class AdminShell
{
public static readonly (string Key, string Label)[] Tabs =
        [("insights", "Insights"), ("students", "Students"), ("supervisors", "Supervisors"), ("projects", "Projects")];

    public static string Normalize(string? tab) =>
        Tabs.FirstOrDefault(t => t.Key == tab?.ToLowerInvariant()).Key ?? Tabs[0].Key;

    public static string TitleOf(string key) => Tabs.First(t => t.Key == key).Label;

    [Parameter, EditorRequired] public string Current { get; set; } = "";
    [Parameter, EditorRequired] public string BasePath { get; set; } = "";
    [Parameter, EditorRequired] public string UserName { get; set; } = "";
    [Parameter] public AdminData? Data { get; set; }
    [Parameter] public string? Error { get; set; }
    [Parameter, EditorRequired] public AdminActions Actions { get; set; } = default!;
    [Parameter] public string? ItemId { get; set; }
    [Parameter] public EventCallback OnSignOut { get; set; }

    [Parameter] public EventCallback OnRetry { get; set; }

    [Parameter] public EventCallback OnProjectsChanged { get; set; }

    private readonly ExportHost exports = new();

    private Guid? RecordId => Guid.TryParse(ItemId, out var id) ? id : null;

    private static string IconFor(string key) => key switch
    {
        "students" => "graduation-cap",
        "supervisors" => "user-round",
        "projects" => "folder-open",
        _ => "layout-dashboard",
    };

    private string HrefFor(string key) => key == "insights" ? BasePath : $"{BasePath}/{key}";
}
