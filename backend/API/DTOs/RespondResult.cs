namespace API.DTOs;

// The inquiry's status after the answer, and the other offers the student was dropped from.
public record RespondResult(Models.InquiryStatus Status, List<Guid> DroppedInquiryIds);
