using Microsoft.AspNetCore.Components;
using MudBlazor;
using uoProjects.Auth;
using uoProjects.Models;

namespace uoProjects.Layout;

public partial class SiteShell
{
    [Inject] private AuthSession Auth { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    [Parameter, EditorRequired] public string Current { get; set; } = "";
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private SignedInUser? user;

    private record NavItem(string Key, string Label, string Icon);

    private static readonly NavItem[] Items =
    [
        new("home", "Home", Icons.Material.Outlined.Home),
        new("projects", "Projects", Icons.Material.Outlined.Science),
        new("supervisors", "Supervisors", Icons.Material.Outlined.Groups),
    ];

    protected override async Task OnInitializedAsync()
    {
        var current = await Auth.GetUserAsync();
        if (current is null)
        {
            Nav.NavigateTo("/login", replace: true);
        }
        else if (current.Role == "admin")
        {
            Nav.NavigateTo(AuthSession.HomeFor(current.Role), replace: true);
        }
        else
        {
            user = current;
        }
    }

    private string HrefFor(string key) => key == "home" ? AuthSession.HomeFor(user!.Role) : key;

    private async Task SignOutAsync()
    {
        await Auth.SignOutAsync();
        Nav.NavigateTo("/login", replace: true);
    }
}
