using System.ComponentModel.DataAnnotations;
using API.Models;

namespace API.DTOs.Request;

public class CreateUploadUrl
{
    [Required, EnumDataType(typeof(AttachmentKind))]
    public AttachmentKind? Kind { get; set; }
}
