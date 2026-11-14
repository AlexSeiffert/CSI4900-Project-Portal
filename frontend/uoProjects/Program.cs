using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using uoProjects;
using uoProjects.Auth;
using uoProjects.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices(options =>
{
    options.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    options.SnackbarConfiguration.VisibleStateDuration = 5000;
    options.SnackbarConfiguration.RequireInteraction = false;
});
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped(services => new AuthSession(new HttpClient { Timeout = TimeSpan.FromSeconds(30) },
    builder.Configuration, services.GetRequiredService<Microsoft.JSInterop.IJSRuntime>()));
builder.Services.AddScoped(services =>
{
    var address = new Uri(builder.Configuration["Api:BaseUrl"]!.TrimEnd('/') + "/");
    return new HttpClient(new ApiAuthorizationHandler(address, services.GetRequiredService<AuthSession>()))
    {
        BaseAddress = address,
        Timeout = TimeSpan.FromSeconds(30)
    };
});
builder.Services.AddScoped<ProjectsService>();
builder.Services.AddScoped<StudentsService>();
builder.Services.AddScoped<SupervisorsService>();
builder.Services.AddScoped<InquiriesService>();
builder.Services.AddScoped<TopicsService>();
builder.Services.AddScoped<SavedProjectsService>();

await builder.Build().RunAsync();
