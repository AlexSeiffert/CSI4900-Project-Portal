using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using API.Auth;
using API.Models;
using API.Models.Data;
using API.Middleware;
using API.Repositories;
using API.Repositories.Interfaces;
using API.Services;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();
// Every endpoint requires a logged-in user.
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

// When a user logs in, Supabase gives them a token. The API checks that token on every request.
// Auth:Authority is your Supabase project's auth address. The API uses it to check tokens are real.
// Auth:JwtSecret is only needed for older Supabase projects.
var authority = builder.Configuration["Auth:Authority"];
var jwtSecret = builder.Configuration["Auth:JwtSecret"];
var isDevelopment = builder.Environment.IsDevelopment();
// Outside Development, the API won't start without one of those two settings.
if (!isDevelopment && !builder.Environment.IsEnvironment("Testing") && string.IsNullOrEmpty(authority) && string.IsNullOrEmpty(jwtSecret))
{
    throw new InvalidOperationException("Set Auth__Authority or Auth__JwtSecret to validate Supabase tokens.");
}
var authentication = builder.Services.AddAuthentication(isDevelopment ? "Smart" : JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.RequireHttpsMetadata = !isDevelopment;
        options.Authority = string.IsNullOrEmpty(authority) ? null : authority;
        options.TokenValidationParameters.ValidAudience = builder.Configuration["Auth:Audience"] ?? "authenticated";
        options.TokenValidationParameters.ValidateAudience = true;
        if (!string.IsNullOrEmpty(jwtSecret))
        {
            options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            options.TokenValidationParameters.ValidateIssuer = !string.IsNullOrEmpty(authority);
        }
    });
if (isDevelopment)
{
    // Development only: lets you skip logging in. Send an X-Dev-User-Id header to act as that user.
    // A real token in the Authorization header still works.
    authentication.AddScheme<AuthenticationSchemeOptions, DevelopmentAuthHandler>(DevelopmentAuthHandler.SchemeName, null)
        .AddPolicyScheme("Smart", "Bearer or development", options =>
            options.ForwardDefaultSelector = context => context.Request.Headers.Authorization.Count > 0
                ? JwtBearerDefaults.AuthenticationScheme
                : DevelopmentAuthHandler.SchemeName);
}
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

var provider = builder.Configuration["Database:Provider"] ?? "Postgres";
var inMemory = provider.Equals("InMemory", StringComparison.OrdinalIgnoreCase);
if (!inMemory && !provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException("Database:Provider must be InMemory or Postgres.", new ArgumentException("Unsupported configured provider.", "Database:Provider"));
}
builder.Services.AddScoped<RlsConnectionInterceptor>();
builder.Services.AddDbContext<ApplicationDbContext>((services, options) =>
{
    if (inMemory)
    {
        options.UseInMemoryDatabase("ProjectPortal");
    }
    else
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Set ConnectionStrings__DefaultConnection for PostgreSQL.", new ArgumentException("The database connection string is missing.", "ConnectionStrings:DefaultConnection")),
        postgres => postgres.MapEnum<ProjectStatus>("project_status", "public"))
            .AddInterceptors(services.GetRequiredService<RlsConnectionInterceptor>());
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
    app.MapOpenApi().AllowAnonymous();
    app.UseSwaggerUi(options => options.DocumentPath = "/openapi/v1.json");
    if (inMemory && builder.Configuration.GetValue<bool>("Database:SeedDevelopmentData"))
    {
        using var scope = app.Services.CreateScope();
        await DevelopmentData.Seed(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
    }
}
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
