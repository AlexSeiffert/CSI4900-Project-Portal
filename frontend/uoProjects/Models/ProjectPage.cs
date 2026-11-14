namespace uoProjects.Models;

public enum ProjectSort { Project, Supervisor, Status, Inquiries, Saves, Updated }

public sealed record ProjectPageQuery(
    int Page = 1, int PageSize = 25, string Search = "", Guid? TopicId = null,
    ProjectStatus? Status = null, ProjectSort Sort = ProjectSort.Project, bool Descending = false);

public sealed record ProjectPageRow(Project Project, string SupervisorName, int InquiryCount, int SaveCount);

public sealed record ProjectPage(List<ProjectPageRow> Items, int Total, Dictionary<ProjectStatus, int> StatusCounts);
