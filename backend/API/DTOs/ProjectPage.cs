using API.Models;

namespace API.DTOs;

public sealed record ProjectPage(List<ProjectPageRow> Items, int Total, Dictionary<ProjectStatus, int> StatusCounts);
