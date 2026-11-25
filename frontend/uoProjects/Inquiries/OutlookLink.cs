using uoProjects.Models;

namespace uoProjects.Inquiries;

public static class OutlookLink
{
    public const string ComposeUrl = "https://outlook.office.com/mail/deeplink/compose";

    public static string ReviewPath(Guid inquiryId) => $"supervisor-home/inquiries/{inquiryId}";

    // To: the supervisor. CC: every other member. The body is the message plus the one review link.
    public static string Build(Inquiry inquiry, Guid senderId, string baseUri)
    {
        var to = inquiry.Supervisor?.ContactEmail ?? "";
        var cc = string.Join(",", (inquiry.Members ?? [])
            .Where(member => member.StudentId != senderId && !string.IsNullOrEmpty(member.Email))
            .Select(member => member.Email));
        var subject = $"Honours project request: {inquiry.Project?.Title ?? "General inquiry"}";
        var link = baseUri.TrimEnd('/') + "/" + ReviewPath(inquiry.Id);
        var body = $"{inquiry.Message}\n\nReview our request, résumés and transcripts here: {link}".TrimStart();
        return $"{ComposeUrl}?to={Uri.EscapeDataString(to)}&cc={Uri.EscapeDataString(cc)}" +
               $"&subject={Uri.EscapeDataString(subject)}&body={Uri.EscapeDataString(body)}";
    }
}
