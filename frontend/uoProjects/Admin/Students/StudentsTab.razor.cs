using System.Web;
using Microsoft.AspNetCore.Components;
using uoProjects.Components;
using uoProjects.Models;

namespace uoProjects.Admin;

public partial class StudentsTab
{
    [Inject] private NavigationManager Nav { get; set; } = default!;

    [Parameter, EditorRequired] public AdminData Data { get; set; } = default!;
    [Parameter, EditorRequired] public string BasePath { get; set; } = "";

    private string query = "";
    private string filter = "all";
    private List<FilterChips.Option> filters = [];

    private static readonly Dictionary<string, Func<InquiryStatus?, bool>> Keeps = new()
    {
        ["all"] = _ => true,
        ["not-started"] = s => s is null,
        ["sent"] = s => s == InquiryStatus.Sent,
        ["matched"] = s => s == InquiryStatus.Matched,
        ["closed"] = s => s == InquiryStatus.Closed,
    };

    private List<Student> shown => Data.Students
        .Where(s => Keeps[filter](Data.FurthestStage(s.UserId)))
        .Where(s => s.DisplayName.Contains(query.Trim(), StringComparison.OrdinalIgnoreCase))
        .ToList();

    protected override void OnInitialized()
    {
        var from = HttpUtility.ParseQueryString(new Uri(Nav.Uri).Query)["filter"];
        if (from is not null && Keeps.ContainsKey(from))
        {
            filter = from;
        }
    }

    protected override void OnParametersSet()
    {
        var stages = Data.Students.Select(s => Data.FurthestStage(s.UserId)).ToList();
        int Count(string key) => stages.Count(Keeps[key]);
        filters =
        [
            new("all", "All", Count("all")),
            new("not-started", "No inquiries", Count("not-started")),
            new("sent", "Sent", Count("sent")),
            new("matched", "Matched", Count("matched")),
            new("closed", "Closed, no match", Count("closed")),
        ];
    }
}
