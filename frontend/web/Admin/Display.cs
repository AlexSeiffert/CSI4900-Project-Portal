using ProjectPortal.Web.Api;

namespace ProjectPortal.Web.Admin;

/// <summary>Words and colours for statuses and times, shared by every admin view so they always agree.</summary>
public static class Display
{
    public static (string Label, string Color) Inquiry(InquiryStatus status) => status switch
    {
        InquiryStatus.Pending => ("Pending", "var(--color-stage-1)"),
        InquiryStatus.Viewed => ("Viewed", "var(--color-stage-2)"),
        InquiryStatus.Responded => ("Responded", "var(--color-stage-3)"),
        _ => ("Closed", "var(--color-stage-4)"),
    };

    /// <summary>A student's progress, named after their furthest inquiry (null: they haven't sent one).</summary>
    public static (string Label, string Color) Stage(InquiryStatus? furthest) => furthest switch
    {
        null => ("No inquiries", "var(--color-chart-neutral)"),
        InquiryStatus.Pending => ("Sent", "var(--color-stage-1)"),
        InquiryStatus.Viewed => ("Viewed", "var(--color-stage-2)"),
        InquiryStatus.Responded => ("Talking", "var(--color-stage-3)"),
        _ => ("Matched", "var(--color-stage-4)"),
    };

    public static (string Label, string Color) Project(ProjectStatus status) => status switch
    {
        ProjectStatus.Available => ("Open", "var(--color-mono-4)"),
        ProjectStatus.Unavailable => ("Unavailable", "var(--color-mono-2)"),
        _ => ("Archived", "var(--color-chart-neutral)"),
    };

    /// <summary>"Today", "1 day ago" or "12 days ago".</summary>
    public static string Ago(DateTime time) => DaysAgo((int)AdminData.Age(time).TotalDays);

    public static string DaysAgo(int days) => days switch
    {
        <= 0 => "Today",
        1 => "1 day ago",
        _ => $"{days} days ago",
    };
}
