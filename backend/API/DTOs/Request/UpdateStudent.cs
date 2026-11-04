using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Request;

public class UpdateStudent
{
    [Required]
    public string DisplayName { get; set; } = string.Empty;
    public string? Program { get; set; }
    [Required]
    public List<Guid> TopicIds { get; set; } = [];
}
