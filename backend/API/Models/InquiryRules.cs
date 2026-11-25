namespace API.Models;

public static class InquiryRules
{
    public const int MaxMembers = 3;

    // Where an offered inquiry ends up once its members have answered: null while anyone is still
    // pending, matched if at least one accepted, closed if nobody did. Mirrors limit_inquiry_updates() in 0009.
    public static InquiryStatus? Outcome(IEnumerable<InquiryMember> members)
    {
        var responses = members.Select(member => member.Response).ToList();
        if (responses.Contains(InquiryResponse.Pending))
        {
            return null;
        }
        return responses.Contains(InquiryResponse.Accepted) ? InquiryStatus.Matched : InquiryStatus.Closed;
    }
}

public record RespondOutcome(InquiryStatus Status, List<Guid> DroppedInquiryIds);
