using API.Models;

namespace API.DTOs;

public sealed record ProjectPageRow(Project Project, string SupervisorName, int InquiryCount, int SaveCount);

public sealed record ProjectPage(List<ProjectPageRow> Items, int Total, Dictionary<ProjectStatus, int> StatusCounts);
