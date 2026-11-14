namespace uoProjects.Exceptions;

public sealed class ApiProblem
{
    public string? Detail { get; init; }
    public string? TraceId { get; init; }
    public Dictionary<string, string[]> Errors { get; init; } = [];
}
