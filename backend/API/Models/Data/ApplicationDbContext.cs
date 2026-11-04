using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.NameTranslation;

namespace API.Models.Data;

// SQL in db/migrations owns the schema. Never call Migrate or EnsureCreated here.
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Supervisor> Supervisors => Set<Supervisor>();
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Inquiry> Inquiries => Set<Inquiry>();
    public DbSet<InquiryAttachment> InquiryAttachments => Set<InquiryAttachment>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.HasDefaultSchema("public");
        model.Entity<Student>().ToTable("student_profiles").HasKey(x => x.UserId);
        model.Entity<Supervisor>().ToTable("supervisor_profiles").HasKey(x => x.UserId);
        model.Entity<Topic>().ToTable("topics");
        model.Entity<Project>().ToTable("projects");
        model.Entity<Inquiry>().ToTable("inquiries");
        model.Entity<InquiryAttachment>().ToTable("inquiry_attachments");
        model.Entity<Student>().Property(x => x.UserId).ValueGeneratedNever();
        model.Entity<Supervisor>().Property(x => x.UserId).ValueGeneratedNever();
        model.Entity<Supervisor>().Property(x => x.AcceptingInquiries).HasDefaultValue(true);
        model.Entity<Project>().HasOne(x => x.Supervisor).WithMany(x => x.Projects)
            .HasForeignKey(x => x.SupervisorId).OnDelete(DeleteBehavior.Cascade);
        model.Entity<Inquiry>().HasOne(x => x.Student).WithMany(x => x.Inquiries)
            .HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Cascade);
        model.Entity<Inquiry>().HasOne(x => x.Supervisor).WithMany(x => x.Inquiries)
            .HasForeignKey(x => x.SupervisorId).OnDelete(DeleteBehavior.Cascade);
        model.Entity<Inquiry>().HasOne(x => x.Project).WithMany(x => x.Inquiries)
            .HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.SetNull);
        model.Entity<InquiryAttachment>().HasOne(x => x.Inquiry).WithMany(x => x.Attachments)
            .HasForeignKey(x => x.InquiryId).OnDelete(DeleteBehavior.Cascade);
        model.Entity<InquiryAttachment>().HasIndex(x => x.StoragePath).IsUnique();

        model.Entity<Project>().HasMany(x => x.Topics).WithMany(x => x.Projects)
            .UsingEntity<Dictionary<string, object>>("project_topic_tags",
                r => r.HasOne<Topic>().WithMany().HasForeignKey("topic_id").OnDelete(DeleteBehavior.Cascade),
                l => l.HasOne<Project>().WithMany().HasForeignKey("project_id").OnDelete(DeleteBehavior.Cascade),
                j => { j.ToTable("project_topic_tags"); j.HasKey("project_id", "topic_id"); });
        model.Entity<Student>().HasMany(x => x.TopicInterests).WithMany(x => x.Students)
            .UsingEntity<Dictionary<string, object>>("student_topic_interests",
                r => r.HasOne<Topic>().WithMany().HasForeignKey("topic_id").OnDelete(DeleteBehavior.Cascade),
                l => l.HasOne<Student>().WithMany().HasForeignKey("student_id").OnDelete(DeleteBehavior.Cascade),
                j => { j.ToTable("student_topic_interests"); j.HasKey("student_id", "topic_id"); });
        model.Entity<Supervisor>().HasMany(x => x.TopicInterests).WithMany(x => x.Supervisors)
            .UsingEntity<Dictionary<string, object>>("supervisor_topic_interests",
                r => r.HasOne<Topic>().WithMany().HasForeignKey("topic_id").OnDelete(DeleteBehavior.Cascade),
                l => l.HasOne<Supervisor>().WithMany().HasForeignKey("supervisor_id").OnDelete(DeleteBehavior.Cascade),
                j => { j.ToTable("supervisor_topic_interests"); j.HasKey("supervisor_id", "topic_id"); });
        model.Entity<Student>().HasMany(x => x.SavedProjects).WithMany()
            .UsingEntity<Dictionary<string, object>>("student_saved_projects",
                r => r.HasOne<Project>().WithMany().HasForeignKey("project_id").OnDelete(DeleteBehavior.Cascade),
                l => l.HasOne<Student>().WithMany().HasForeignKey("student_id").OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.ToTable("student_saved_projects"); j.HasKey("student_id", "project_id");
                    j.Property<DateTime>("saved_at").HasDefaultValueSql("now()");
                });

        var names = new NpgsqlSnakeCaseNameTranslator();
        foreach (var entity in model.Model.GetEntityTypes())
        {
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(names.TranslateMemberName(property.Name));
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetDefaultValueSql("now()");
                }

                if (Database.IsNpgsql() && property.Name == "UpdatedAt")
                {
                    // Read back the value produced by the database's BEFORE UPDATE trigger.
                    property.ValueGenerated = ValueGenerated.OnAddOrUpdate;
                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
                }
                if (property.Name == "Id" && property.ClrType == typeof(Guid))
                {
                    property.SetDefaultValueSql("gen_random_uuid()");
                }
            }
        }
        // Keep the existing C# name, but map to SQL's posted_at (there is no created_at).
        model.Entity<Project>().Property(x => x.CreatedAt).HasColumnName("posted_at");
        model.Entity<Project>().Property(x => x.Status).HasColumnType("public.project_status")
            .HasDefaultValue(ProjectStatus.Available);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // PostgreSQL owns update timestamps through triggers. Mirror them for local InMemory use.
        foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Modified))
        {
            if (entry.Entity is Project or Inquiry or Student or Supervisor)
            {
                entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
