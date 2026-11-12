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
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false)));
builder.Services.AddOpenApi(options => options.AddDocumentTransformer((document, context, cancellationToken) =>
{
    document.Components ??= new OpenApiComponents();
    document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
    document.Security ??= [];
    document.Components.SecuritySchemes[JwtBearerDefaults.AuthenticationScheme] = new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "A Supabase access token (see \"Log in to Swagger\" in backend/README.md)."
    };
    document.Security.Add(new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, document)] = [] });
    if (context.ApplicationServices.GetRequiredService<IHostEnvironment>().IsDevelopment())
    {
        document.Components.SecuritySchemes[DevelopmentAuthHandler.SchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Name = DevelopmentAuthHandler.UserIdHeader,
            Description = "Development only: act as this user id without logging in. Ignored if a token is also set."
        };
        document.Security.Add(new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference(DevelopmentAuthHandler.SchemeName, document)] = [] });
    }
    return Task.CompletedTask;
}));
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

var authority = builder.Configuration["Auth:Authority"];
var jwtSecret = builder.Configuration["Auth:JwtSecret"];
var isDevelopment = builder.Environment.IsDevelopment();
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
    authentication.AddScheme<AuthenticationSchemeOptions, DevelopmentAuthHandler>(DevelopmentAuthHandler.SchemeName, null)
        .AddPolicyScheme("Smart", "Bearer or development", options =>
            options.ForwardDefaultSelector = context => context.Request.Headers.Authorization.Count > 0
                ? JwtBearerDefaults.AuthenticationScheme
                : DevelopmentAuthHandler.SchemeName);
}
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
{
    context.ProblemDetails.Instance ??= context.HttpContext.Request.Path;
    context.ProblemDetails.Extensions["traceId"] = System.Diagnostics.Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
});

builder.Services.AddExceptionHandler<ApiExceptionHandler>();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

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
        postgres => postgres.MapEnum<ProjectStatus>("project_status", "public").MapEnum<InquiryStatus>("inquiry_status", "public"))
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
    app.UseSwaggerUi(options =>
    {
        options.DocumentPath = "/openapi/v1.json";
        options.PersistAuthorization = true;
    });
    if (inMemory && builder.Configuration.GetValue<bool>("Database:SeedDevelopmentData"))
    {
        using var scope = app.Services.CreateScope();
        await DevelopmentData.Seed(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
    }
}
app.UseHttpsRedirection();
app.UseStatusCodePages();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
