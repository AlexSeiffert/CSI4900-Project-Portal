using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Request;

public class AddInquiryMember
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
