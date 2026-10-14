using API.Models;
using API.Models.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace API.Tests.Support;

public class PortalApiFactory : WebApplicationFactory<Program>
{
    public static readonly Guid SupervisorId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid OtherSupervisorId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid StudentId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid OtherStudentId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid TopicId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid OtherTopicId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((context, configuration) => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Database:Provider"] = "InMemory",
                ["Database:SeedDevelopmentData"] = "false"
            }));
        builder.ConfigureServices(services =>
        {
            // The final options registration selects a unique database for each test host.
            services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(_databaseName));
        });
    }

    public async Task<HttpClient> CreateSeededClient()
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Supervisors.AddRange(
            new Supervisor { UserId = SupervisorId, DisplayName = "Supervisor", ContactEmail = "supervisor@example.test" },
            new Supervisor { UserId = OtherSupervisorId, DisplayName = "Other Supervisor", ContactEmail = "other@example.test" });
        db.Students.AddRange(
            new Student { UserId = StudentId, DisplayName = "Student" },
            new Student { UserId = OtherStudentId, DisplayName = "Other Student" });
        db.Topics.AddRange(new Topic { Id = TopicId, Name = "Software Engineering" },
            new Topic { Id = OtherTopicId, Name = "Databases" });
        await db.SaveChangesAsync();
        return client;
    }
}
