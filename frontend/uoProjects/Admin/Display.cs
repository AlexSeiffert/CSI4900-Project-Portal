using uoProjects.Models;

namespace uoProjects.Admin;

public static class Display
{
    public static (string Label, string Color) Inquiry(InquiryStatus status)
    {
        return status switch
        {
            InquiryStatus.Sent => ("Sent", "var(--color-stage-1)"),
            InquiryStatus.Matched => ("Matched", "var(--color-stage-4)"),
            _ => ("Closed, no match", "var(--color-muted)"),
        };
    }

    public static (string Label, string Color) Stage(InquiryStatus? furthest)
    {
        return furthest switch
        {
            null => ("No inquiries", "var(--color-chart-neutral)"),
            InquiryStatus.Sent => ("Sent", "var(--color-stage-1)"),
            InquiryStatus.Matched => ("Matched", "var(--color-stage-4)"),
            _ => ("Closed, no match", "var(--color-muted)"),
        };
    }

    public static (string Label, string Color) Project(ProjectStatus status)
    {
        return status switch
        {
            ProjectStatus.Available => ("Open", "var(--color-mono-4)"),
            ProjectStatus.Unavailable => ("Unavailable", "var(--color-mono-2)"),
            _ => ("Archived", "var(--color-chart-neutral)"),
        };
    }

    public static string Ago(DateTime time)
    {
        return DaysAgo((int)AdminData.Age(time).TotalDays);
    }

    public static string DaysAgo(int days)
    {
        return days switch
        {
            <= 0 => "Today",
            1 => "1 day ago",
            _ => $"{days} days ago",
        };
    }
}
