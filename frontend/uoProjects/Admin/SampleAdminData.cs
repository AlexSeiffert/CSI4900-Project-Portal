using uoProjects.Api;
using uoProjects.Components;

namespace uoProjects.Admin;

public static class SampleAdminData
{
    private static readonly string[] Surnames =
        ["Okafor", "Lindqvist", "Haddad", "Moreau", "Tanaka", "Brennan", "Achebe", "Kowalski", "Singh", "Varga",
         "Nakamura", "Ferreira", "Dubois", "Mensah", "Petrov", "Larsen", "Castillo", "Huang", "O'Neill", "Rossi"];
    private static readonly string[] FirstNames =
        ["Amara", "Liam", "Sofia", "Noah", "Zara", "Ethan", "Maya", "Omar", "Chloé", "Lucas", "Priya", "Félix",
         "Hana", "Mateo", "Aisha", "Samuel", "Léa", "Kai", "Nadia", "Theo"];
    private static readonly string[] Titles =
        ["Explainable models for", "A dashboard for", "Privacy-preserving tools for", "Benchmarking", "Accessible interfaces for",
         "Low-power sensing for", "Automated testing of", "Real-time visualisation of"];
    private static readonly string[] Subjects =
        ["campus energy use", "transit delays", "clinical notes", "student wellbeing", "open-source repositories",
         "wildlife cameras", "smart-home devices", "course scheduling", "satellite imagery", "river water quality"];

    public static AdminData Create()
    {
        var random = new Random(4900);
        var now = DateTime.UtcNow;
        DateTime DaysAgo(int max) => now.AddDays(-random.Next(1, max)).AddHours(-random.Next(24));
        Guid NextGuid()
        {
            var bytes = new byte[16];
            random.NextBytes(bytes);
            return new Guid(bytes);
        }

        var topics = TopicPalette.Names.Select(name => new Topic(NextGuid(), name)).ToList();

        var supervisors = Surnames.Select((name, i) => new Supervisor(
            NextGuid(), $"Dr. {name}", $"{name.ToLowerInvariant().Replace("'", "")}@uottawa.ca", null, null,
            AcceptingInquiries: i % 10 >= 3, DaysAgo(60), null)).ToList();

        var students = Enumerable.Range(0, 120).Select(i => new Student(
            NextGuid(), $"{FirstNames[i % FirstNames.Length]} {Surnames[(i * 7 + 3) % Surnames.Length]}",
            DaysAgo(40), null)).ToList();

        var projects = Enumerable.Range(0, 80).Select(i => new Project(
            NextGuid(), supervisors[i % supervisors.Count].UserId,
            $"{Titles[i % Titles.Length]} {Subjects[(i * 3) % Subjects.Length]}", "", null,
            i < 47 ? ProjectStatus.Available : i < 68 ? ProjectStatus.Unavailable : ProjectStatus.Archived,
            DaysAgo(150), DaysAgo(i % 9 == 0 ? 120 : 50), null)).ToList();

        var inquiries = new List<Inquiry>();
        foreach (var student in students.Skip(18))
        {
            for (var n = random.Next(1, 4); n > 0; n--)
            {
                var project = projects[random.Next(47)];
                var roll = random.Next(100);
                var status = roll < 55 ? InquiryStatus.Sent : roll < 80 ? InquiryStatus.Matched : InquiryStatus.Closed;
                var created = status == InquiryStatus.Sent && random.Next(4) > 0 ? DaysAgo(7) : DaysAgo(35);
                inquiries.Add(new Inquiry(NextGuid(), student.UserId, project.SupervisorId, project.Id, null, status, created, created));
            }
        }

        var projectTopics = projects.ToDictionary(p => p.Id, p => topics
            .OrderBy(t => t.Name == "AI/ML" ? random.Next(3) : random.Next(10)).Take(random.Next(1, 3)).ToList());

        var saveCounts = projects.ToDictionary(p => p.Id, _ => random.Next(0, 9));

        return new AdminData(students, supervisors, projects, inquiries, topics, projectTopics, saveCounts);
    }
}
