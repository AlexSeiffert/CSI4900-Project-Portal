namespace API.Models;

public class InquiryMember
{
    public Guid InquiryId { get; set; }

    public Guid StudentId { get; set; }

    public InquiryResponse Response { get; set; } = InquiryResponse.Pending;

    public Inquiry Inquiry { get; set; } = null!;

    public Student Student { get; set; } = null!;
}
