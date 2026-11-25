using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Request;

public class CreateInquiry
{
    public const int MessageMaxLength = 2000;

    // The client does not choose this. If a value is sent, it is ignored (JsonIgnore),
    // and the controller sets it to the id of the logged-in student.
    [System.Text.Json.Serialization.JsonIgnore]
    public Guid StudentId { get; set; }

    // The project's supervisor becomes the inquiry's supervisor.
    [Required]
    public Guid? ProjectId { get; set; }

    [MaxLength(MessageMaxLength)]
    public string? Message { get; set; }
}
