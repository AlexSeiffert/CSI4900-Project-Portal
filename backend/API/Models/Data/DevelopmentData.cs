using Microsoft.EntityFrameworkCore;

namespace API.Models.Data;

internal static class DevelopmentData
{
    public static async Task Seed(ApplicationDbContext db)
    {
        if (await db.Supervisors.AnyAsync())
        {
            return;
        }

        var now = DateTime.UtcNow;
        var topics = new[]
        {
            new Topic { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Software Engineering" },
            new Topic { Id = Guid.Parse("33333333-3333-3333-3333-333333333334"), Name = "Artificial Intelligence" },
            new Topic { Id = Guid.Parse("33333333-3333-3333-3333-333333333335"), Name = "Databases" },
            new Topic { Id = Guid.Parse("33333333-3333-3333-3333-333333333336"), Name = "Cybersecurity" },
            new Topic { Id = Guid.Parse("33333333-3333-3333-3333-333333333337"), Name = "Web Development" },
            new Topic { Id = Guid.Parse("33333333-3333-3333-3333-333333333338"), Name = "Human-Computer Interaction" },
            new Topic { Id = Guid.Parse("33333333-3333-3333-3333-333333333339"), Name = "Distributed Systems" },
            new Topic { Id = Guid.Parse("33333333-3333-3333-3333-333333333340"), Name = "Accessibility" }
        };

        var supervisors = new[]
        {
            new Supervisor
            {
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                DisplayName = "Dr. Elena Chen",
                ContactEmail = "elena.chen@example.test",
                AboutMe = "Researching maintainable software, accessible web applications, and developer tools.",
                ExternalUrl = "https://example.test/research/elena-chen",
                TopicInterests = [topics[0], topics[4], topics[7]]
            },
            new Supervisor
            {
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111112"),
                DisplayName = "Dr. Marcus Ahmed",
                ContactEmail = "marcus.ahmed@example.test",
                AboutMe = "Applied machine learning and data management for university services.",
                ExternalUrl = "https://example.test/research/marcus-ahmed",
                TopicInterests = [topics[1], topics[2], topics[5]]
            },
            new Supervisor
            {
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111113"),
                DisplayName = "Dr. Priya Patel",
                ContactEmail = "priya.patel@example.test",
                AboutMe = "Security, privacy, and reliable distributed applications.",
                AcceptingInquiries = false,
                TopicInterests = [topics[3], topics[6]]
            },
            new Supervisor
            {
                UserId = Guid.Parse("11111111-1111-1111-1111-111111111114"),
                DisplayName = "Dr. Noah Williams",
                ContactEmail = "noah.williams@example.test"
                // A profile with no projects, inquiries, or interests.
            }
        };

        var students = new[]
        {
            new Student
            {
                UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                DisplayName = "Alex Morgan",
                TopicInterests = [topics[0], topics[4]]
            },
            new Student
            {
                UserId = Guid.Parse("22222222-2222-2222-2222-222222222223"),
                DisplayName = "Maya Singh",
                TopicInterests = [topics[1], topics[2]]
            },
            new Student
            {
                UserId = Guid.Parse("22222222-2222-2222-2222-222222222224"),
                DisplayName = "Liam Brooks",
                TopicInterests = [topics[3], topics[6]]
            },
            new Student
            {
                UserId = Guid.Parse("22222222-2222-2222-2222-222222222225"),
                DisplayName = "Sofia Garcia"
                // Empty interests, inquiries, and saved projects.
            },
            new Student
            {
                UserId = Guid.Parse("22222222-2222-2222-2222-222222222226"),
                DisplayName = "Oliver Martin",
                TopicInterests = [topics[5], topics[7]]
            }
        };

        var projects = new[]
        {
            new Project
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444441"), Supervisor = supervisors[0],
                Title = "Accessible Project Portal",
                Description = "Build an accessible web portal for discovering honours projects and supervisors.",
                ExternalUrl = "https://example.test/projects/accessible-portal",
                Status = ProjectStatus.Available, Topics = [topics[0], topics[4], topics[7]]
            },
            new Project
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444442"), Supervisor = supervisors[0],
                Title = "Automated API Testing Toolkit",
                Description = "Develop reusable integration tests and OpenAPI contract checks for ASP.NET Core services.",
                Status = ProjectStatus.Unavailable, Topics = [topics[0], topics[4]]
            },
            new Project
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444443"), Supervisor = supervisors[0],
                Title = "Campus Usability Study",
                Description = "Evaluate navigation and usability in campus web applications with student participants.",
                Status = ProjectStatus.Archived, Topics = [topics[5], topics[7]]
            },
            new Project
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Supervisor = supervisors[1],
                Title = "Honours Project Recommendation Engine",
                Description = "Use machine learning to match students with projects based on their research interests.",
                ExternalUrl = "https://example.test/projects/recommendations",
                Status = ProjectStatus.Available, Topics = [topics[1], topics[2]]
            },
            new Project
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444445"), Supervisor = supervisors[1],
                Title = "Research Data Search",
                Description = "Build a searchable PostgreSQL index and compare ranking strategies for research datasets.",
                Status = ProjectStatus.Available, Topics = [topics[2], topics[4]]
            },
            new Project
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444446"), Supervisor = supervisors[1],
                Title = "Explainable Student Support Classifier",
                Description = "Study interpretable AI models and communicate their limitations through a usable interface.",
                Status = ProjectStatus.Unavailable, Topics = [topics[1], topics[5]]
            },
            new Project
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444447"), Supervisor = supervisors[2],
                Title = "Privacy-Aware Document Sharing",
                Description = "Investigate access controls and auditability for confidential research documents.",
                Status = ProjectStatus.Available, Topics = [topics[3], topics[4]]
            },
            new Project
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444448"), Supervisor = supervisors[2],
                Title = "Distributed Job Scheduler",
                Description = "Prototype a fault-tolerant background job scheduler and evaluate failure recovery.",
                Status = ProjectStatus.Archived, Topics = [topics[6], topics[0]]
            },
            new Project
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444449"), Supervisor = supervisors[2],
                Title = "Exploratory Security Research",
                Description = "Define an independent security research question with the supervisor.",
                Status = ProjectStatus.Available
                // No topic tags, inquiries, saved relationships, or external URL.
            }
        };

        for (var index = 0; index < projects.Length; index++)
        {
            projects[index].SupervisorId = projects[index].Supervisor.UserId;
            projects[index].CreatedAt = now.AddDays(-60 + index * 3);
            projects[index].UpdatedAt = projects[index].CreatedAt.AddDays(2);
        }

        var inquiries = new[]
        {
            new Inquiry
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555551"),
                Status = InquiryStatus.Sent,
                Student = students[0], Supervisor = supervisors[0], Project = projects[0],
                Message = "We are interested in accessible web development. Could we discuss this project?"
            },
            new Inquiry
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555552"),
                Status = InquiryStatus.Offered,
                Student = students[1], Supervisor = supervisors[1], Project = projects[3],
                Message = "We have completed a machine learning course and would like to work on recommendations."
            },
            new Inquiry
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555553"),
                Status = InquiryStatus.Matched,
                Student = students[2], Supervisor = supervisors[2], Project = projects[6],
                Message = "I would like to explore privacy-preserving access controls."
            },
            new Inquiry
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555554"),
                Status = InquiryStatus.Closed,
                Student = students[0], Supervisor = supervisors[0], Project = projects[1],
                Message = "Is there room for another student on the testing toolkit?"
            },
            new Inquiry
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Status = InquiryStatus.Draft,
                Student = students[1], Supervisor = supervisors[0], Project = projects[0],
                Message = "Are there upcoming projects that combine AI and software engineering?"
            },
            new Inquiry
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555556"),
                Status = InquiryStatus.Sent,
                Student = students[2], Supervisor = supervisors[1]
                // A general inquiry with a null project and message.
            },
            new Inquiry
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555557"),
                Status = InquiryStatus.Sent,
                Student = students[4], Supervisor = supervisors[0], Project = projects[2],
                Message = "Thank you for discussing the results of the campus usability study."
            },
            new Inquiry
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555558"),
                Status = InquiryStatus.Offered,
                Student = students[4], Supervisor = supervisors[1], Project = projects[4],
                Message = "Could the search interface support keyboard-only navigation?"
            }
        };

        for (var index = 0; index < inquiries.Length; index++)
        {
            inquiries[index].StudentId = inquiries[index].Student!.UserId;
            inquiries[index].SupervisorId = inquiries[index].Supervisor!.UserId;
            inquiries[index].ProjectId = inquiries[index].Project?.Id;
            inquiries[index].CreatedAt = now.AddDays(-14 + index);
            inquiries[index].UpdatedAt = inquiries[index].CreatedAt.AddHours(1);
        }

        // The creator is always the first member. Alex is pending on two offers (2 and 8), so
        // accepting one drops Alex from the other.
        (int Inquiry, int Student, InquiryResponse Response)[] members =
        [
            (0, 0, InquiryResponse.Pending), (0, 3, InquiryResponse.Pending),
            (1, 1, InquiryResponse.Accepted), (1, 0, InquiryResponse.Pending), (1, 4, InquiryResponse.Pending),
            (2, 2, InquiryResponse.Accepted),
            (3, 0, InquiryResponse.Pending),
            (4, 1, InquiryResponse.Pending),
            (5, 2, InquiryResponse.Pending),
            (6, 4, InquiryResponse.Pending),
            (7, 4, InquiryResponse.Pending), (7, 0, InquiryResponse.Pending)
        ];
        foreach (var (inquiry, student, response) in members)
        {
            inquiries[inquiry].Members.Add(new InquiryMember { Student = students[student], StudentId = students[student].UserId, Response = response });
        }

        // Login emails, so friends can be added by email. Postgres keeps these in public.users.
        var users = students.Select(student => new UserAccount
            {
                Id = student.UserId,
                Email = student.DisplayName.ToLowerInvariant().Replace(' ', '.') + "@example.test"
            })
            .Concat(supervisors.Select(supervisor => new UserAccount { Id = supervisor.UserId, Email = supervisor.ContactEmail }))
            .ToList();

        // Saving a project is independent of sending an inquiry.
        students[0].SavedProjects = [projects[0], projects[3], projects[4]];
        students[1].SavedProjects = [projects[3], projects[4], projects[6]];
        students[2].SavedProjects = [projects[6], projects[7]];
        students[4].SavedProjects = [projects[0], projects[2]];

        db.Topics.AddRange(topics);
        db.Supervisors.AddRange(supervisors);
        db.Students.AddRange(students);
        db.Projects.AddRange(projects);
        db.Inquiries.AddRange(inquiries);
        db.Users.AddRange(users);

        // The InMemory provider does not evaluate the SQL default for saved_at.
        db.ChangeTracker.DetectChanges();
        foreach (var entry in db.ChangeTracker.Entries().Where(entry =>
                     entry.Metadata.GetTableName() == "student_saved_projects" && entry.State == EntityState.Added))
        {
            entry.Property("saved_at").CurrentValue = now.AddDays(-2);
        }

        await db.SaveChangesAsync();
    }
}
