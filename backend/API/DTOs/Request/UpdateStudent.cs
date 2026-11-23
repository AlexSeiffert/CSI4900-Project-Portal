using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Request;

public class UpdateStudent
{
    [Required]
    public string DisplayName { get; set; } = string.Empty;
    [Required]
    public List<Guid> TopicIds { get; set; } = [];
}
