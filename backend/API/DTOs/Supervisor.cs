namespace API.DTOs;

public class Supervisor : UserDto
{
    public string ContactEmail { get; set; } = string.Empty;
    public string? AboutMe { get; set; }
    public string? ExternalUrl { get; set; }
    public bool AcceptingInquiries { get; set; }
    public List<Topic> TopicInterests { get; set; } = [];
    public List<Project> Projects { get; set; } = [];
    public List<Inquiry> Inquiries { get; set; } = [];
}
