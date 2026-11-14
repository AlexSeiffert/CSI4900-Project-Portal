using Microsoft.AspNetCore.Components;
using uoProjects.Auth;
using uoProjects.Models;
using System.ComponentModel.DataAnnotations;

namespace uoProjects.Pages;

public partial class Login
{
    [Inject] private AuthSession Auth { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

private static readonly EmailAddressAttribute emailRule = new();
    private readonly LoginModel model = new();
    private bool emailValid;
    private bool busy;
    private string? error;

    protected override async Task OnInitializedAsync()
    {
        if (await Auth.GetUserAsync() is { } user)
        {
            Nav.NavigateTo(AuthSession.HomeFor(user.Role), replace: true);
        }
    }

    private async Task SubmitAsync()
    {
        busy = true;
        error = await Auth.SignInAsync(model.Email, model.Password);
        busy = false;

        if (error is null && await Auth.GetUserAsync() is { } user)
        {
            Nav.NavigateTo(AuthSession.HomeFor(user.Role), replace: true);
        }
    }

    private sealed class LoginModel
    {
        [Required(ErrorMessage = "Enter your email.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Enter your password.")]
        public string Password { get; set; } = "";
    }
}
