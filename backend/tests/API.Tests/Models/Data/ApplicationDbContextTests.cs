using API.Models;
using API.Models.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace API.Tests.Models.Data;

public class ApplicationDbContextTests
{
    [Fact]
    public void NpgsqlModelMatchesSqlSchemaAndTranslatesFilters()
    {
        // Validate the real Npgsql model and query translation without opening a database connection.
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=unused;Username=unused", postgres =>
                postgres.MapEnum<ProjectStatus>("project_status", "public")
                        .MapEnum<InquiryStatus>("inquiry_status", "public"))
            .Options;
        using var db = new ApplicationDbContext(options);
        var model = db.Model;
        var expected = new Dictionary<string, string[]>
        {
            ["student_profiles"] = ["user_id", "display_name", "program", "updated_at"],
            ["supervisor_profiles"] = ["user_id", "display_name", "contact_email", "about_me", "external_url", "accepting_inquiries", "updated_at"],
            ["topics"] = ["id", "name"],
            ["projects"] = ["id", "supervisor_id", "title", "description", "external_url", "status", "posted_at", "updated_at"],
            ["inquiries"] = ["id", "student_id", "supervisor_id", "project_id", "message", "status", "created_at", "updated_at"],
            ["inquiry_attachments"] = ["id", "inquiry_id", "storage_path", "mime_type", "size_bytes", "created_at"],
            ["project_topic_tags"] = ["project_id", "topic_id"],
            ["student_topic_interests"] = ["student_id", "topic_id"],
            ["supervisor_topic_interests"] = ["supervisor_id", "topic_id"],
            ["student_saved_projects"] = ["student_id", "project_id", "saved_at"]
        };
        Assert.Equal(expected.Count, model.GetEntityTypes().Count());
        foreach (var entity in model.GetEntityTypes())
        {
            var table = entity.GetTableName()!;
            Assert.Equal("public", entity.GetSchema());
            Assert.Equal(expected[table].Order(), entity.GetProperties().Select(p => p.GetColumnName()).Order());
        }
        var project = model.FindEntityType(typeof(Project))!;
        Assert.Equal("public.project_status", project.FindProperty(nameof(Project.Status))!.GetColumnType());
        Assert.Equal(ValueGenerated.OnAddOrUpdate, project.FindProperty(nameof(Project.UpdatedAt))!.ValueGenerated);
        var inquiry = model.FindEntityType(typeof(Inquiry))!;
        Assert.True(inquiry.FindProperty(nameof(Inquiry.Message))!.IsNullable);
        Assert.True(inquiry.FindProperty(nameof(Inquiry.ProjectId))!.IsNullable);
        Assert.Equal(DeleteBehavior.SetNull, inquiry.GetForeignKeys().Single(f => f.PrincipalEntityType == project).DeleteBehavior);
        Assert.Equal(typeof(Guid), model.FindEntityType(typeof(Topic))!.FindProperty(nameof(Topic.Id))!.ClrType);
        var saved = model.GetEntityTypes().Single(e => e.GetTableName() == "student_saved_projects");
        Assert.Equal(["student_id", "project_id"], saved.FindPrimaryKey()!.Properties.Select(p => p.Name));
        Assert.All(saved.GetForeignKeys(), foreignKey => Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior));
        var topicId = Guid.NewGuid();
        var sql = db.Projects.Include(p => p.Topics)
            .Where(p => p.Status == ProjectStatus.Available && p.Topics.Any(t => t.Id == topicId)
                && (p.Title.ToLower().Contains("portal") || p.Description.ToLower().Contains("portal")))
            .OrderByDescending(p => p.CreatedAt).ToQueryString();
        Assert.Contains("posted_at", sql);
        Assert.Contains("project_topic_tags", sql);
        Assert.Contains("project_status", sql);
    }
}
