using System.ComponentModel.DataAnnotations;
using API.Models;

namespace API.DTOs.Request;

public enum ProjectSort { Project, Supervisor, Status, Inquiries, Saves, Updated }

public sealed class ProjectPageQuery
{
    [Range(1, int.MaxValue / 100)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 25;

    public string? Search { get; set; }
    public Guid? TopicId { get; set; }

    [EnumDataType(typeof(ProjectStatus))]
    public ProjectStatus? Status { get; set; }

    [EnumDataType(typeof(ProjectSort))]
    public ProjectSort Sort { get; set; }

    public bool Descending { get; set; }
}
