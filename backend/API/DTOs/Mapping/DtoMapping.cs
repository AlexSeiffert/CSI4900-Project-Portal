namespace API.DTOs.Mapping;

internal static class DtoMapping
{
    // ToDto maps only the object's own fields. ToDtoWithRelations also adds its relationships,
    // one level deep: those related objects come back plain, without their own lists. So an inquiry
    // includes its student, but not the student's other inquiries. Call that endpoint for those.
    // This keeps responses small, avoids loops, and never shows a half-loaded list as if it were complete.
    // Endpoints opt in to relationships with ToDtoWithRelations and a matching repository query.

    public static Topic ToDto(this Models.Topic x) => new() { Id = x.Id, Name = x.Name };

    public static Project ToDto(this Models.Project x) => new()
    {
        Id = x.Id, SupervisorId = x.SupervisorId, Title = x.Title,
        Description = x.Description, ExternalUrl = x.ExternalUrl, Status = x.Status,
        CreatedAt = x.CreatedAt, UpdatedAt = x.UpdatedAt
    };

    public static Inquiry ToDto(this Models.Inquiry x) => new()
    {
        Id = x.Id, StudentId = x.StudentId, SupervisorId = x.SupervisorId,
        ProjectId = x.ProjectId, Message = x.Message, Status = x.Status,
        CreatedAt = x.CreatedAt, UpdatedAt = x.UpdatedAt
    };

    public static Student ToDto(this Models.Student x) => new()
    {
        UserId = x.UserId, DisplayName = x.DisplayName,
        Program = x.Program, UpdatedAt = x.UpdatedAt
    };

    public static Supervisor ToDto(this Models.Supervisor x) => new()
    {
        UserId = x.UserId, DisplayName = x.DisplayName,
        ContactEmail = x.ContactEmail, AboutMe = x.AboutMe,
        ExternalUrl = x.ExternalUrl, AcceptingInquiries = x.AcceptingInquiries,
        UpdatedAt = x.UpdatedAt
    };

    public static Topic ToDtoWithRelations(this Models.Topic x)
    {
        var dto = x.ToDto();
        dto.Projects = x.Projects.Select(project => project.ToDto()).ToList();
        dto.Students = x.Students.Select(student => student.ToDto()).ToList();
        dto.Supervisors = x.Supervisors.Select(supervisor => supervisor.ToDto()).ToList();
        return dto;
    }

    public static Project ToDtoWithRelations(this Models.Project x)
    {
        var dto = x.ToDto();
        dto.Supervisor = x.Supervisor?.ToDto();
        dto.Topics = x.Topics.Select(topic => topic.ToDto()).ToList();
        dto.Inquiries = x.Inquiries.Select(inquiry => inquiry.ToDto()).ToList();
        return dto;
    }

    public static Inquiry ToDtoWithRelations(this Models.Inquiry x)
    {
        var dto = x.ToDto();
        dto.Student = x.Student?.ToDto();
        dto.Supervisor = x.Supervisor?.ToDto();
        dto.Project = x.Project?.ToDto();
        return dto;
    }

    public static Student ToDtoWithRelations(this Models.Student x)
    {
        var dto = x.ToDto();
        dto.TopicInterests = x.TopicInterests.Select(topic => topic.ToDto()).ToList();
        dto.Inquiries = x.Inquiries.Select(inquiry => inquiry.ToDto()).ToList();
        dto.SavedProjects = x.SavedProjects.Select(project => project.ToDto()).ToList();
        return dto;
    }

    public static Supervisor ToDtoWithRelations(this Models.Supervisor x)
    {
        var dto = x.ToDto();
        dto.TopicInterests = x.TopicInterests.Select(topic => topic.ToDto()).ToList();
        dto.Projects = x.Projects.Select(project => project.ToDto()).ToList();
        dto.Inquiries = x.Inquiries.Select(inquiry => inquiry.ToDto()).ToList();
        return dto;
    }
}
