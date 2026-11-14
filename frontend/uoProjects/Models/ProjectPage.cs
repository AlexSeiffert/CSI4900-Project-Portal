namespace uoProjects.Models;

public sealed record ProjectPage(
    List<ProjectPageRow> Items,
    int Total,
    Dictionary<ProjectStatus, int> StatusCounts
);
