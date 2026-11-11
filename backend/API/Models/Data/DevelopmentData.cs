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
            new Topic { Id = Guid.Parse("33333333-3333-3333-3333-333333333334"), Name = "AI/ML" },
            new Topic { Id = Guid.Parse("33333333-3333-3333-3333-333333333335"), Name = "Data & Databases" },
            new Topic { Id = Guid.Parse("33333333-3333-3333-3333-333333333336"), Name = "Security & Privacy" },
            new Topic { Id = Guid.Parse("33333333-3333-3333-3333-333333333337"), Name = "Web & Mobile" },
            new Topic { Id = Guid.Parse("33333333-3333-3333-3333-333333333338"), Name = "HCI & Design" },
            new Topic { Id = Guid.Parse("33333333-3333-3333-3333-333333333339"), Name = "Systems & Networks" },
            new Topic { Id = Guid.Parse("33333333-3333-3333-3333-333333333340"), Name = "Games & Graphics" },
            new Topic { Id = Guid.Parse("33333333-3333-3333-3333-333333333341"), Name = "Theory & Algorithms" },
            new Topic { Id = Guid.Parse("33333333-3333-3333-3333-333333333342"), Name = "Other" }
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
                Student = students[0], Supervisor = supervisors[0], Project = projects[0],
                Message = "I am interested in accessible web development. Could we discuss this project?"
            },
            new Inquiry
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555552"),
                Student = students[1], Supervisor = supervisors[1], Project = projects[3],
                Message = "I have completed a machine learning course and would like to work on recommendations."
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
                Student = students[1], Supervisor = supervisors[0],
                Message = "Are there upcoming projects that combine AI and software engineering?"
            },
            new Inquiry
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555556"),
                Student = students[2], Supervisor = supervisors[1]
            },
            new Inquiry
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555557"),
                Student = students[4], Supervisor = supervisors[0], Project = projects[2],
                Message = "Thank you for discussing the results of the campus usability study."
            },
            new Inquiry
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555558"),
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

        students[0].SavedProjects = [projects[0], projects[3], projects[4]];
        students[1].SavedProjects = [projects[3], projects[4], projects[6]];
        students[2].SavedProjects = [projects[6], projects[7]];
        students[4].SavedProjects = [projects[0], projects[2]];

        db.Topics.AddRange(topics);
        db.Supervisors.AddRange(supervisors);
        db.Students.AddRange(students);
        db.Projects.AddRange(projects);
        db.Inquiries.AddRange(inquiries);
        AddGeneratedData(db, topics, now);

        db.ChangeTracker.DetectChanges();
        foreach (var entry in db.ChangeTracker.Entries().Where(entry =>
                     entry.Metadata.GetTableName() == "student_saved_projects" && entry.State == EntityState.Added))
        {
            entry.Property("saved_at").CurrentValue = now.AddDays(-2);
        }

        await db.SaveChangesAsync();
    }

    private static void AddGeneratedData(ApplicationDbContext db, Topic[] topics, DateTime now)
    {
        var random = new Random(4900);
        var pickTopics = (int first, int extra) => new[] { first }
            .Concat(Enumerable.Range(0, topics.Length).Where(i => i != first).OrderBy(_ => random.Next()).Take(extra))
            .Select(i => topics[i]).ToList();

        var supervisors = SupervisorNames.Select((name, index) =>
        {
            var interests = pickTopics(index % topics.Length, random.Next(0, 3));
            var areas = string.Join(", ", interests.Select(t => t.Name));
            return new Supervisor
            {
                UserId = Guid.Parse($"11111111-0000-0000-0000-{index + 1:D12}"),
                DisplayName = $"Dr. {name}",
                ContactEmail = $"{name.ToLowerInvariant().Replace(' ', '.').Replace("'", "")}@example.test",
                AboutMe = $"Works on {areas}. Supervising {random.Next(2, 6)} honours projects this year.",
                ExternalUrl = random.NextDouble() < 0.5 ? $"https://example.test/research/{index + 1}" : null,
                AcceptingInquiries = random.NextDouble() < 0.8,
                TopicInterests = interests
            };
        }).ToList();

        var projects = ProjectIdeas.Select((idea, index) =>
        {
            var owners = supervisors.Where(s => s.TopicInterests.Contains(topics[idea.Topic])).ToList();
            var owner = owners[index % owners.Count];
            var tags = new List<Topic> { topics[idea.Topic] };
            var second = owner.TopicInterests.FirstOrDefault(t => t != topics[idea.Topic]);
            if (second is not null && random.NextDouble() < 0.35)
            {
                tags.Add(second);
            }
            var roll = random.Next(100);
            var created = now.AddDays(-random.Next(30, 111));
            var updated = created.AddDays(random.Next(1, 30));
            return new Project
            {
                Id = Guid.Parse($"44444444-0000-0000-0000-{index + 1:D12}"),
                Supervisor = owner, SupervisorId = owner.UserId,
                Title = idea.Title, Description = idea.Description,
                ExternalUrl = random.NextDouble() < 0.25 ? $"https://example.test/projects/{index + 1}" : null,
                Status = roll < 70 ? ProjectStatus.Available : roll < 86 ? ProjectStatus.Unavailable : ProjectStatus.Archived,
                Topics = tags,
                CreatedAt = created,
                UpdatedAt = updated > now ? now : updated
            };
        }).ToList();

        var names = new HashSet<string>();
        var students = new List<Student>();
        while (students.Count < 65)
        {
            var name = $"{FirstNames[random.Next(FirstNames.Length)]} {LastNames[random.Next(LastNames.Length)]}";
            if (!names.Add(name))
            {
                continue;
            }
            var roll = random.Next(100);
            students.Add(new Student
            {
                UserId = Guid.Parse($"22222222-0000-0000-0000-{students.Count + 1:D12}"),
                DisplayName = name,
                TopicInterests = pickTopics(random.Next(topics.Length), random.Next(0, 3))
            });
        }

        var open = projects.Where(p => p.Status == ProjectStatus.Available).ToList();
        var choose = (Student student, int count) => open
            .OrderByDescending(p => random.NextDouble() * (p.Topics.Any(student.TopicInterests.Contains) ? 4 : 1))
            .Take(count).ToList();
        var inquiries = new List<Inquiry>();
        foreach (var student in students)
        {
            student.SavedProjects = choose(student, random.Next(0, 6));
            if (random.NextDouble() < 0.15)
            {
                continue;
            }
            foreach (var project in choose(student, random.Next(1, 4)))
            {
                var roll = random.Next(100);
                var created = now.AddDays(-random.Next(0, 43)).AddHours(-random.Next(0, 24));
                var updated = created.AddHours(random.Next(1, 72));
                var general = random.NextDouble() < 0.08;
                inquiries.Add(new Inquiry
                {
                    Id = Guid.Parse($"55555555-0000-0000-0000-{inquiries.Count + 1:D12}"),
                    Student = student, StudentId = student.UserId,
                    Supervisor = project.Supervisor, SupervisorId = project.SupervisorId,
                    Project = general ? null : project, ProjectId = general ? null : project.Id,
                    Status = roll < 55 ? InquiryStatus.Sent : roll < 85 ? InquiryStatus.Matched : InquiryStatus.Closed,
                    Message = InquiryMessages[random.Next(InquiryMessages.Length)],
                    CreatedAt = created,
                    UpdatedAt = updated > now ? now : updated
                });
            }
        }

        db.Supervisors.AddRange(supervisors);
        db.Students.AddRange(students);
        db.Projects.AddRange(projects);
        db.Inquiries.AddRange(inquiries);
    }

    private static readonly string?[] InquiryMessages =
    [
        "Hi, I'm really interested in this project. I've done related coursework and would love to talk about it.",
        "Hello! Could we meet to discuss this project? I have experience with the tools you mention.",
        "I'd like to work on this for my honours project. Is it still available?",
        "This lines up with what I want to do after graduating. Do you have time for a short meeting this week?",
        null
    ];

    private static readonly string[] SupervisorNames =
    [
        "Amira Haddad", "Daniel Okafor", "Ingrid Lindqvist", "Julien Moreau", "Kenji Tanaka", "Siobhan Brennan",
        "Chinedu Achebe", "Marta Kowalski", "Harpreet Singh", "Zsofia Varga", "Yuki Nakamura", "Rafael Ferreira",
        "Claire Dubois", "Kwame Mensah", "Dmitri Petrov", "Astrid Larsen", "Lucia Castillo", "Wei Huang",
        "Fiona O'Neill", "Giulia Rossi", "Thomas Bergeron", "Nadia Rahman", "Samuel Cohen", "Leila Farouk",
        "Mohammed Al-Sayed", "Hannah Kowalczyk"
    ];

    private static readonly string[] FirstNames =
    [
        "Aisha", "Ben", "Chloé", "Diego", "Elif", "Félix", "Grace", "Hamza", "Isla", "Jonah", "Kiara", "Lucas",
        "Mei", "Nikhil", "Olivia", "Pierre", "Qing", "Rania", "Sami", "Talia", "Umar", "Vera", "William", "Ximena",
        "Yusuf", "Zoë", "Arjun", "Bianca", "Camille", "Dev", "Emma", "Farah", "Gabriel", "Hana", "Ivan", "Jade"
    ];

    private static readonly string[] LastNames =
    [
        "Abdi", "Bouchard", "Chen", "Desjardins", "Esposito", "Fortin", "Gagnon", "Hassan", "Ibrahim", "Joshi",
        "Kim", "Lavoie", "Nguyen", "Osei", "Qureshi", "Roy", "Santos", "Tremblay", "Usman", "Vasquez", "Wong",
        "Xu", "Yilmaz", "Zhang", "Ali", "Côté", "Diallo", "Gauthier", "Kaur", "Lefebvre", "Pelletier"
    ];

    private static readonly (int Topic, string Title, string Description)[] ProjectIdeas =
    [
        (0, "Flaky test detection in CI pipelines", "Find and classify flaky tests in large open-source repositories using their CI history."),
        (0, "A refactoring assistant for legacy Java", "Detect code smells in legacy Java projects and suggest safe automated refactorings."),
        (0, "Property-based testing for REST APIs", "Generate test cases from OpenAPI specs and find bugs that example-based tests miss."),
        (0, "A teaching compiler for a tiny language", "Design a small language and write a compiler to WebAssembly with friendly error messages."),
        (0, "Static analysis for accessibility bugs", "Write lint rules that catch common accessibility mistakes in React code before it ships."),
        (1, "Detecting misinformation in French and English news", "Train and compare multilingual language models that flag misleading claims in Canadian news articles."),
        (1, "An LLM teaching assistant for first-year programming", "Build a retrieval-augmented assistant that answers questions using only the course notes, and measure how often it is right."),
        (1, "Counting birds from trail camera images", "Use computer vision to identify and count bird species from camera traps along the Rideau."),
        (1, "Reinforcement learning for traffic light timing", "Simulate a downtown intersection and train an agent that reduces average wait times."),
        (1, "Sign language recognition from webcam video", "Recognise a vocabulary of LSQ and ASL signs in real time on a laptop."),
        (1, "Predicting OC Transpo delays", "Combine GTFS data with weather to forecast bus delays an hour ahead."),
        (2, "Visualising Ottawa's open data", "Interactive visualisations of city service requests, built for residents rather than analysts."),
        (2, "Query optimisation for time-series data", "Compare indexing strategies for sensor data in PostgreSQL and TimescaleDB."),
        (2, "Search engine for course materials", "Index lecture slides and notes so students can search across all their courses."),
        (2, "Data quality checks for research datasets", "Automatically detect missing, duplicated and inconsistent records in shared datasets."),
        (2, "Streaming analytics for energy meters", "Detect unusual energy use in campus buildings as readings arrive."),
        (3, "Phishing detection for university email", "Build and evaluate a classifier for phishing emails using header and content features."),
        (3, "Privacy audit of popular mobile apps", "Measure what personal data popular Canadian apps send to third parties."),
        (3, "Post-quantum TLS performance", "Benchmark post-quantum key exchange in TLS on low-power devices."),
        (3, "Smart contract vulnerability scanner", "Detect common vulnerabilities in Solidity contracts with static analysis."),
        (3, "Differential privacy for course analytics", "Publish useful course statistics without exposing individual students."),
        (4, "Offline-first field notes app for ecologists", "A mobile app that records observations with photos and GPS, and syncs when back online."),
        (4, "Accessible campus wayfinding", "Indoor navigation for campus buildings with step-free routes and screen-reader support."),
        (4, "A booking system for student clubs", "A full-stack app for clubs to book rooms and equipment, with conflict detection."),
        (4, "Cross-platform medication reminders", "A Flutter app for older adults, designed and tested with real users."),
        (4, "Food bank inventory tracker", "A web app for a local food bank to track donations, stock and pickups."),
        (5, "VR exposure therapy prototype", "A VR environment for practising public speaking, designed with a clinical psychologist."),
        (5, "Accessible data charts for screen readers", "Make charts understandable through sound and structured descriptions."),
        (5, "AR assembly instructions", "Augmented-reality instructions for lab equipment, compared against paper manuals."),
        (5, "Designing for older adults' video calls", "Simplify video calling for older adults living alone, with in-home testing."),
        (6, "Energy-aware scheduling in Kubernetes", "Schedule workloads to cut energy use without missing deadlines."),
        (6, "LoRaWAN sensors for river monitoring", "Deploy low-power sensors that report water level and temperature from the Ottawa River."),
        (6, "Measuring 5G latency on campus", "Map real-world latency and throughput across campus and explain the dead zones."),
        (6, "A fault-tolerant key-value store", "Implement Raft and test it under network partitions with fault injection."),
        (6, "GPU acceleration for image processing", "Port an image-processing pipeline to CUDA and measure the speed-up."),
        (7, "Procedural terrain for a survival game", "Generate varied, playable terrain with noise functions and erosion simulation."),
        (7, "A serious game for teaching recursion", "A puzzle game that teaches recursion, tested in first-year labs."),
        (7, "Real-time cloth simulation", "Simulate cloth on the GPU with position-based dynamics."),
        (7, "Multiplayer netcode for fast action games", "Compare rollback and lockstep networking under real network conditions."),
        (8, "Exam timetabling as constraint solving", "Model the faculty exam timetable as a constraint problem and beat the current schedule."),
        (8, "Quantum algorithms on real hardware", "Run small quantum algorithms on cloud quantum computers and study the noise."),
        (8, "Verifying a sorting library in Dafny", "Formally verify correctness of a small sorting library."),
        (9, "Robot arm that sorts recycling", "Teach a low-cost robot arm to pick and sort recyclables on a conveyor."),
        (9, "Auto-grading for programming labs", "Give students instant, useful feedback on lab submissions."),
        (9, "Drone mapping for search and rescue", "Plan drone flight paths that cover search areas quickly.")
    ];
}
