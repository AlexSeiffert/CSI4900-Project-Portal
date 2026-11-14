using Microsoft.AspNetCore.Components;
using uoProjects.Models;

namespace uoProjects.Auth;

public partial class RoleGate
{
    [Inject] private AuthSession Auth { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    [Parameter, EditorRequired] public string Role { get; set; } = "";
    [Parameter, EditorRequired] public RenderFragment<SignedInUser> ChildContent { get; set; } = default!;

    private SignedInUser? user;

    protected override async Task OnInitializedAsync()
    {
        var current = await Auth.GetUserAsync();
        if (current is null)
        {
            Nav.NavigateTo("/login", replace: true);
        }
        else if (current.Role != Role)
        {
            Nav.NavigateTo(AuthSession.HomeFor(current.Role), replace: true);
        }
        else
        {
            user = current;
        }
    }
}
