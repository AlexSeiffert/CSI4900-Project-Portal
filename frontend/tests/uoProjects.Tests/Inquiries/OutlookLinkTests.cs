using uoProjects.Inquiries;
using uoProjects.Models;

namespace uoProjects.Tests.Inquiries;

public class OutlookLinkTests
{
    private static readonly Guid Sender = Guid.NewGuid();

    private static Inquiry Request(string? message = "We'd like to join.")
    {
        var supervisor = new Supervisor(Guid.NewGuid(), "Dr. Chen", "chen@uottawa.ca", null, null, true, default, null);
        return new Inquiry(Guid.NewGuid(), Sender, supervisor.UserId, Guid.NewGuid(), message, InquiryStatus.Draft, default, default,
            [
                new InquiryMember(Sender, InquiryResponse.Pending, "Me", "me@uottawa.ca"),
                new InquiryMember(Guid.NewGuid(), InquiryResponse.Pending, "Friend", "friend@uottawa.ca"),
                new InquiryMember(Guid.NewGuid(), InquiryResponse.Pending, "Other", "other@uottawa.ca")
            ],
            supervisor, new Project(Title: "Accessible portal"));
    }

    private static Dictionary<string, string> Query(string url)
    {
        Assert.StartsWith(OutlookLink.ComposeUrl + "?", url);
        return new Uri(url).Query.TrimStart('?').Split('&')
            .Select(part => part.Split('=', 2))
            .ToDictionary(pair => pair[0], pair => Uri.UnescapeDataString(pair[1]));
    }

    [Fact]
    public void Build_AddressesTheSupervisorAndCcsTheOtherMembers()
    {
        var query = Query(OutlookLink.Build(Request(), Sender, "https://app.test/"));

        Assert.Equal("chen@uottawa.ca", query["to"]);
        Assert.Equal("friend@uottawa.ca,other@uottawa.ca", query["cc"]);
        Assert.Contains("Accessible portal", query["subject"]);
    }

    [Fact]
    public void Build_BodyIsTheMessagePlusOneReviewLink()
    {
        var inquiry = Request();

        var body = Query(OutlookLink.Build(inquiry, Sender, "https://app.test/"))["body"];

        Assert.StartsWith("We'd like to join.", body);
        Assert.Single(System.Text.RegularExpressions.Regex.Matches(body, "https://"));
        Assert.EndsWith($"https://app.test/supervisor-home/inquiries/{inquiry.Id}", body);
    }

    [Fact]
    public void Build_EscapesSpecialCharacters()
    {
        var url = OutlookLink.Build(Request("Hi & thanks?\nSecond line"), Sender, "https://app.test/");

        Assert.DoesNotContain("\n", url);
        Assert.StartsWith("Hi & thanks?\nSecond line", Query(url)["body"]);
    }
}
