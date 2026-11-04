using System.ComponentModel.DataAnnotations;
using API.Models;

namespace API.DTOs.Request;

public class UpdateProject
{
    [Required]
    public string Title { get; set; } = string.Empty;
    [Required]
    public string Description { get; set; } = string.Empty;
    [Url]
    public string? ExternalUrl { get; set; }
    [EnumDataType(typeof(ProjectStatus))]
    public ProjectStatus Status { get; set; } = ProjectStatus.Available;
    [Required]
    public List<Guid> TopicIds { get; set; } = [];
}
