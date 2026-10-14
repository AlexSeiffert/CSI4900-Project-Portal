namespace API.DTOs;

public class Student : UserDto
{
    public string? Program { get; set; }
    public List<Topic> TopicInterests { get; set; } = [];
    public List<Inquiry> Inquiries { get; set; } = [];
    public List<Project> SavedProjects { get; set; } = [];
}
