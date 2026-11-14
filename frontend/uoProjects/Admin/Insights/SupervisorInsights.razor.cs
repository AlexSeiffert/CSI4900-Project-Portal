using Microsoft.AspNetCore.Components;
using uoProjects.Admin;
using uoProjects.Models;

namespace uoProjects.Admin.Insights;

public partial class SupervisorInsights
{
private const int Top = 10;
    private const int ChartHeight = Top * 30 + 50;

    [Parameter, EditorRequired] public AdminData Data { get; set; } = default!;

    private object load = new();
    private object rates = new();
    private string rateNote = "";

    protected override void OnParametersSet()
    {
        var busiest = Data.Supervisors.OrderByDescending(s => Data.InquiriesTo(s.UserId).Count()).Take(Top).ToList();
        load = new
        {
            categories = busiest.Select(s => s.DisplayName),
            horizontal = true,
            stacked = true,
            series = Enum.GetValues<InquiryStatus>().Select(status => new
            {
                name = Display.Inquiry(status).Label,
                color = Display.Inquiry(status).Color,
                data = busiest.Select(s => Data.InquiriesTo(s.UserId).Count(i => i.Status == status)),
            }),
        };

        var accepting = Data.Supervisors.Where(s => s.AcceptingInquiries).ToList();
        var matchRates = accepting.Select(s => Data.MatchRate(s.UserId)).ToList();
        var ranges = Enumerable.Range(0, 5)
            .Select(b => (Label: $"{b * 20}–{b * 20 + (b == 4 ? 20 : 19)}%",
                          Count: matchRates.Count(r => r is { } rate && Math.Min((int)(rate * 5), 4) == b),
                          Color: $"mono-{b + 1}"))
            .Append((Label: "No inquiries", Count: matchRates.Count(r => r is null), Color: "chart-neutral"))
            .ToList();
        rates = new
        {
            categories = ranges.Select(r => r.Label),
            barWidth = 40,
            legend = false,
            series = new[]
            {
                new { name = "Supervisors", color = "mono-4", data = ranges.Select(r => (object)new { value = r.Count, color = r.Color }) },
            },
        };
        rateNote = $"Distribution of match rates across the {accepting.Count} supervisors accepting inquiries this term.";
    }
}
