using System.Text.Json.Serialization;

namespace API.DTOs;

public class Student : UserDto
{
    public string? Program { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Topic>? TopicInterests { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Inquiry>? Inquiries { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Project>? SavedProjects { get; set; }
}
