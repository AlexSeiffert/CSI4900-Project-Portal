using System.Text.Json;
using System.Text.Json.Serialization;
using API.Models;
using API.Models.Data;
using API.Middleware;
using API.Repositories;
using API.Repositories.Interfaces;
using API.Services;
using API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false)));
builder.Services.AddOpenApi();
builder.Services.AddAuthorization();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

var provider = builder.Configuration["Database:Provider"] ?? "Postgres";
var inMemory = provider.Equals("InMemory", StringComparison.OrdinalIgnoreCase);
if (!inMemory && !provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException("Database:Provider must be InMemory or Postgres.", new ArgumentException("Unsupported configured provider.", "Database:Provider"));
}
// The API is a local development scaffold until authentication and RLS request context are wired.
if (!builder.Environment.IsDevelopment() && !builder.Environment.IsEnvironment("Testing"))
{
    throw new InvalidOperationException("Authentication is not integrated. Run only in Development or Testing until authorization is implemented.", new NotSupportedException("The configured environment requires authentication."));
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (inMemory)
    {
        options.UseInMemoryDatabase("ProjectPortal");
    }
    else
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Set ConnectionStrings__DefaultConnection for PostgreSQL.", new ArgumentException("The database connection string is missing.", "ConnectionStrings:DefaultConnection")),
        postgres => postgres.MapEnum<ProjectStatus>("project_status", "public"));
    }
});
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ISupervisorRepository, SupervisorRepository>();
builder.Services.AddScoped<ITopicRepository, TopicRepository>();
builder.Services.AddScoped<IInquiryRepository, InquiryRepository>();
builder.Services.AddScoped<ISavedProjectRepository, SavedProjectRepository>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ISupervisorService, SupervisorService>();
builder.Services.AddScoped<ITopicService, TopicService>();
builder.Services.AddScoped<IInquiryService, InquiryService>();
builder.Services.AddScoped<ISavedProjectService, SavedProjectService>();

var app = builder.Build();
app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUi(options => options.DocumentPath = "/openapi/v1.json");
    if (inMemory && builder.Configuration.GetValue<bool>("Database:SeedDevelopmentData"))
    {
        using var scope = app.Services.CreateScope();
        await DevelopmentData.Seed(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
    }
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
