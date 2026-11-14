namespace uoProjects.Models;

public sealed record ProjectPageQuery(
    int Page = 1,
    int PageSize = 25,
    string Search = "",
    Guid? TopicId = null,
    ProjectStatus? Status = null,
    ProjectSort Sort = ProjectSort.Project,
    bool Descending = false
);
