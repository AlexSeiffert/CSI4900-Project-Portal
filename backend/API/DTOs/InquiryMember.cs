using System.Text.Json.Serialization;

namespace API.DTOs;

public class InquiryMember
{
    public Guid StudentId { get; set; }
    public Models.InquiryResponse Response { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DisplayName { get; set; }

    // Only on a single inquiry, for the members and its supervisor (the Outlook CC line).
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Email { get; set; }
}
