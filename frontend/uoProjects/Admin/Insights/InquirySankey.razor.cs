using Microsoft.AspNetCore.Components;
using uoProjects.Models;

namespace uoProjects.Admin;

public partial class InquirySankey
{
    [Parameter, EditorRequired] public int Total { get; set; }
    [Parameter, EditorRequired] public int Matched { get; set; }
    [Parameter, EditorRequired] public int Closed { get; set; }
    [Parameter, EditorRequired] public int Open { get; set; }

    private object data = new();

    protected override void OnParametersSet()
    {
        (string Name, int Value, string Color)[] ends =
            [("Matched", Matched, "stage-4"), ("Closed, no match", Closed, "muted"), ("Still open", Open, "chart-neutral")];

        var nodes = new List<object> { new { name = "Sent", value = Total, depth = 0, color = "stage-1" } };
        var links = new List<object>();
        foreach (var end in ends.Where(e => e.Value > 0))
        {
            nodes.Add(new { name = end.Name, value = end.Value, depth = 1, color = end.Color });
            links.Add(new { source = "Sent", target = end.Name, value = end.Value, color = "gradient" });
        }
        data = new { nodes, links };
    }
}
