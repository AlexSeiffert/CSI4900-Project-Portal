namespace API.DTOs.Mapping;

internal static class DtoMapping
{
    public static Topic ToDto(this Models.Topic x)
    {
        return new() { Id = x.Id, Name = x.Name };
    }

    public static Project ToDto(this Models.Project x)
    {
        return new()
        {
            Id = x.Id,
            SupervisorId = x.SupervisorId,
            Title = x.Title,
            Description = x.Description,
            ExternalUrl = x.ExternalUrl,
            Status = x.Status,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
            Topics = x.Topics.Select(ToDto).ToList()
        };
    }

    public static Inquiry ToDto(this Models.Inquiry x)
    {
        return new()
        {
            Id = x.Id,
            StudentId = x.StudentId,
            SupervisorId = x.SupervisorId,
            ProjectId = x.ProjectId,
            Message = x.Message,
            Status = x.Status,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        };
    }

    public static Student ToDto(this Models.Student x)
    {
        return new()
        {
            UserId = x.UserId,
            DisplayName = x.DisplayName,
            Program = x.Program,
            UpdatedAt = x.UpdatedAt,
            TopicInterests = x.TopicInterests.Select(ToDto).ToList()
        };
    }

    public static Supervisor ToDto(this Models.Supervisor x)
    {
        return new()
        {
            UserId = x.UserId,
            DisplayName = x.DisplayName,
            ContactEmail = x.ContactEmail,
            AboutMe = x.AboutMe,
            ExternalUrl = x.ExternalUrl,
            AcceptingInquiries = x.AcceptingInquiries,
            UpdatedAt = x.UpdatedAt,
            TopicInterests = x.TopicInterests.Select(ToDto).ToList()
        };
    }
}
