using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Request;

public class UpdateSupervisor
{
    [Required]
    public string DisplayName { get; set; } = string.Empty;
    [Required, EmailAddress]
    public string ContactEmail { get; set; } = string.Empty;
    public string? AboutMe { get; set; }
    [Url]
    public string? ExternalUrl { get; set; }
    public bool AcceptingInquiries { get; set; } = true;
    [Required]
    public List<Guid> TopicIds { get; set; } = [];
}
