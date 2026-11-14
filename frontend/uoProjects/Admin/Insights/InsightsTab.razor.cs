using System.Web;
using Microsoft.AspNetCore.Components;
using uoProjects.Models;

namespace uoProjects.Admin;

public partial class InsightsTab
{
    [Inject] private NavigationManager Nav { get; set; } = default!;

private static readonly (string, string, string)[] Areas =
    [
        ("inquiries", "Inquiries", "send"),
        ("projects", "Projects", "folder-open"),
        ("topics", "Topics", "tags"),
        ("supervisors", "Supervisors", "users"),
    ];


    [Parameter, EditorRequired] public AdminData Data { get; set; } = default!;
    [Parameter, EditorRequired] public string BasePath { get; set; } = "";

    private string area = "inquiries";
    private int matched, notStarted, stale, matchedInquiries, open, looking;
    private List<ShareBar.Segment> studentStages = [];

    protected override void OnInitialized()
    {
        var from = HttpUtility.ParseQueryString(new Uri(Nav.Uri).Query)["area"];
        if (Areas.Any(a => a.Item1 == from))
        {
            area = from!;
        }
    }

    protected override void OnParametersSet()
    {
        var stages = Data.Students.Select(s => Data.FurthestStage(s.UserId)).ToList();
        matched = stages.Count(s => s == InquiryStatus.Matched);
        notStarted = stages.Count(s => s is null);
        looking = Data.Students.Count - matched;
        stale = Data.Stale.Count();
        matchedInquiries = Data.Inquiries.Count(i => i.Status == InquiryStatus.Matched);
        open = Data.Projects.Count(p => p.Status == ProjectStatus.Available);

        studentStages =
        [
            new("No inquiries", notStarted, "var(--color-chart-neutral)"),
            new("Sent", stages.Count(s => s == InquiryStatus.Sent), "var(--color-stage-1)"),
            new("Closed, no match", stages.Count(s => s == InquiryStatus.Closed), "var(--color-muted)", LightText: true),
            new("Matched", matched, "var(--color-stage-4)", LightText: true),
        ];
    }
}
