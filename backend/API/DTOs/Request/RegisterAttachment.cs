using System.ComponentModel.DataAnnotations;
using API.Models;

namespace API.DTOs.Request;

public class RegisterAttachment
{
    public const int MaxSizeBytes = 2 * 1024 * 1024;

    [Required]
    public string StoragePath { get; set; } = string.Empty;

    [Required, EnumDataType(typeof(AttachmentKind))]
    public AttachmentKind? Kind { get; set; }

    [Required]
    public string MimeType { get; set; } = "application/pdf";

    [Range(1, MaxSizeBytes)]
    public int SizeBytes { get; set; }
}
