using Microsoft.AspNetCore.Components;
using uoProjects.Admin;
using uoProjects.Models;

namespace uoProjects.Admin.Insights;

public partial class InquiryInsights
{
private const int Weeks = 8;

    [Parameter, EditorRequired] public AdminData Data { get; set; } = default!;

    private object weekly = new();

    protected override void OnParametersSet()
    {
        var weeks = Enumerable.Range(0, Weeks).Reverse().ToList();
        int WeekOf(Inquiry i) => (int)(AdminData.Age(i.CreatedAt).TotalDays / 7);
        weekly = new
        {
            categories = weeks.Select(w => DateTime.Now.AddDays(-7 * (w + 1) + 1).ToString("MMM d")),
            stacked = true,
            barWidth = 56,
            series = Enum.GetValues<InquiryStatus>().Select(status => new
            {
                name = Display.Inquiry(status).Label,
                color = Display.Inquiry(status).Color,
                data = weeks.Select(w => Data.Inquiries.Count(i => i.Status == status && WeekOf(i) == w)),
            }),
        };
    }
}
