using System.Text.Json.Serialization;

namespace API.DTOs;

public class Supervisor : UserDto
{
    public string ContactEmail { get; set; } = string.Empty;
    public string? AboutMe { get; set; }
    public string? ExternalUrl { get; set; }
    public bool AcceptingInquiries { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Topic>? TopicInterests { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Project>? Projects { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Inquiry>? Inquiries { get; set; }
}
