using MudBlazor;
using MudBlazor.Services;
using uoProjects.Components;
using uoProjects.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices(options =>
{
    options.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    options.SnackbarConfiguration.VisibleStateDuration = 5000;
    options.SnackbarConfiguration.RequireInteraction = false;
});
builder.Services.AddScoped<NotificationService>();
builder.Services.AddProblemDetails();
builder.Services.AddHttpClient("Api", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"]
        ?? throw new InvalidOperationException("Set Api:BaseUrl to the backend API address."));

    // Use the API's existing development authentication until login is implemented.
    if (builder.Environment.IsDevelopment()
        && !string.IsNullOrWhiteSpace(builder.Configuration["Api:DevUserId"]))
    {
        client.DefaultRequestHeaders.Add("X-Dev-User-Id", builder.Configuration["Api:DevUserId"]);
    }
}).AddTypedClient<ProjectsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
