namespace API.Models;

public class Student
{
    public Guid UserId { get; set; }

    public string DisplayName { get; set; } = string.Empty;


    public DateTime UpdatedAt { get; set; }

    public ICollection<Topic> TopicInterests { get; set; } = new List<Topic>();

    public ICollection<Inquiry> Inquiries { get; set; } = new List<Inquiry>();

    public ICollection<Project> SavedProjects { get; set; } = new List<Project>();
}
