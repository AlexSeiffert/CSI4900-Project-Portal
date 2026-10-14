using System.ComponentModel.DataAnnotations;
using API.Models;

namespace API.DTOs.Request;

public class UpdateInquiry
{
    [Required, EnumDataType(typeof(InquiryStatus))]
    public InquiryStatus? Status { get; set; }
}
