using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components;
using uoProjects.Admin;
using uoProjects.Models;

namespace uoProjects.Pages;

public partial class AdminPreview
{
    [Inject] private IWebAssemblyHostEnvironment Env { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

private static readonly AdminData data = SampleAdminData.Create();
    private static readonly AdminActions actions = new(null);

    [Parameter] public string? Tab { get; set; }
    [Parameter] public string? ItemId { get; set; }

    private string current = "insights";

    protected override void OnParametersSet() => current = AdminShell.Normalize(Tab);
}
