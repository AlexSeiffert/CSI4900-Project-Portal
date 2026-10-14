using System.Text.Json.Serialization;

namespace API.DTOs;

public class Topic : BaseDto<Guid>
{
    public string Name { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Project>? Projects { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Student>? Students { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Supervisor>? Supervisors { get; set; }
}
