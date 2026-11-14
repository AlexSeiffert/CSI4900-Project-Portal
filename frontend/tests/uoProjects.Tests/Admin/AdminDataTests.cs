using uoProjects.Admin;

namespace uoProjects.Tests.Admin;

public class AdminDataTests
{
    [Fact]
    public void PreviewMatchRate_CountsOnlyMatchedInquiries()
    {
        var data = SampleAdminData.Create();
        var supervisor = data.Supervisors.First(s => data.InquiriesTo(s.UserId).Any());
        var inquiries = data.InquiriesTo(supervisor.UserId).ToList();

        var rate = data.MatchRate(supervisor.UserId);

        Assert.Equal(inquiries.Count(i => i.Status == Models.InquiryStatus.Matched) / (double)inquiries.Count, rate);
    }
}
