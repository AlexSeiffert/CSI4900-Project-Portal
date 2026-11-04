namespace API.Models;

public class Topic
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    // Relationships
    public ICollection<Project> Projects { get; set; } = new List<Project>();

    public ICollection<Student> Students { get; set; } = new List<Student>();

    public ICollection<Supervisor> Supervisors { get; set; } = new List<Supervisor>();
}
