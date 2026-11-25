using System.ComponentModel.DataAnnotations;

namespace API.DTOs.Request;

public class RespondToInquiry
{
    // Accepting also drops the student from their other offered inquiries.
    [Required]
    public bool? Accept { get; set; }
}
