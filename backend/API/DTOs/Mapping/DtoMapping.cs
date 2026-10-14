namespace API.DTOs.Mapping;

internal static class DtoMapping
{
    public static Topic ToDto(this Models.Topic x) => new Mapper().Map(x, 0);
    public static Project ToDto(this Models.Project x) => new Mapper().Map(x, 0);
    public static Inquiry ToDto(this Models.Inquiry x) => new Mapper().Map(x, 0);
    public static Student ToDto(this Models.Student x) => new Mapper().Map(x, 0);
    public static Supervisor ToDto(this Models.Supervisor x) => new Mapper().Map(x, 0);

    // Include the requested DTO's relations and their relations, then stop expanding.
    // Objects on the active path are reused so IgnoreCycles can serialize back-references.
    private sealed class Mapper
    {
        private const int MaxRelationshipDepth = 2;
        private readonly Dictionary<object, object> _active = new(ReferenceEqualityComparer.Instance);

        public Topic Map(Models.Topic x, int depth)
        {
            if (_active.TryGetValue(x, out var existing))
            {
                return (Topic)existing;
            }

            var dto = new Topic { Id = x.Id, Name = x.Name };
            _active.Add(x, dto);
            if (depth < MaxRelationshipDepth)
            {
                dto.Projects = x.Projects.Select(project => Map(project, depth + 1)).ToList();
                dto.Students = x.Students.Select(student => Map(student, depth + 1)).ToList();
                dto.Supervisors = x.Supervisors.Select(supervisor => Map(supervisor, depth + 1)).ToList();
            }
            _active.Remove(x);
            return dto;
        }

        public Project Map(Models.Project x, int depth)
        {
            if (_active.TryGetValue(x, out var existing))
            {
                return (Project)existing;
            }

            var dto = new Project
            {
                Id = x.Id, SupervisorId = x.SupervisorId, Title = x.Title,
                Description = x.Description, ExternalUrl = x.ExternalUrl, Status = x.Status,
                CreatedAt = x.CreatedAt, UpdatedAt = x.UpdatedAt
            };
            _active.Add(x, dto);
            if (depth < MaxRelationshipDepth)
            {
                dto.Supervisor = x.Supervisor is null ? null : Map(x.Supervisor, depth + 1);
                dto.Topics = x.Topics.Select(topic => Map(topic, depth + 1)).ToList();
                dto.Inquiries = x.Inquiries.Select(inquiry => Map(inquiry, depth + 1)).ToList();
            }
            _active.Remove(x);
            return dto;
        }

        public Inquiry Map(Models.Inquiry x, int depth)
        {
            if (_active.TryGetValue(x, out var existing))
            {
                return (Inquiry)existing;
            }

            var dto = new Inquiry
            {
                Id = x.Id, StudentId = x.StudentId, SupervisorId = x.SupervisorId,
                ProjectId = x.ProjectId, Message = x.Message,
                CreatedAt = x.CreatedAt, UpdatedAt = x.UpdatedAt
            };
            _active.Add(x, dto);
            if (depth < MaxRelationshipDepth)
            {
                dto.Student = x.Student is null ? null : Map(x.Student, depth + 1);
                dto.Supervisor = x.Supervisor is null ? null : Map(x.Supervisor, depth + 1);
                dto.Project = x.Project is null ? null : Map(x.Project, depth + 1);
            }
            _active.Remove(x);
            return dto;
        }

        public Student Map(Models.Student x, int depth)
        {
            if (_active.TryGetValue(x, out var existing))
            {
                return (Student)existing;
            }

            var dto = new Student
            {
                UserId = x.UserId, DisplayName = x.DisplayName,
                Program = x.Program, UpdatedAt = x.UpdatedAt
            };
            _active.Add(x, dto);
            if (depth < MaxRelationshipDepth)
            {
                dto.TopicInterests = x.TopicInterests.Select(topic => Map(topic, depth + 1)).ToList();
                dto.Inquiries = x.Inquiries.Select(inquiry => Map(inquiry, depth + 1)).ToList();
                dto.SavedProjects = x.SavedProjects.Select(project => Map(project, depth + 1)).ToList();
            }
            _active.Remove(x);
            return dto;
        }

        public Supervisor Map(Models.Supervisor x, int depth)
        {
            if (_active.TryGetValue(x, out var existing))
            {
                return (Supervisor)existing;
            }

            var dto = new Supervisor
            {
                UserId = x.UserId, DisplayName = x.DisplayName,
                ContactEmail = x.ContactEmail, AboutMe = x.AboutMe,
                ExternalUrl = x.ExternalUrl, AcceptingInquiries = x.AcceptingInquiries,
                UpdatedAt = x.UpdatedAt
            };
            _active.Add(x, dto);
            if (depth < MaxRelationshipDepth)
            {
                dto.TopicInterests = x.TopicInterests.Select(topic => Map(topic, depth + 1)).ToList();
                dto.Projects = x.Projects.Select(project => Map(project, depth + 1)).ToList();
                dto.Inquiries = x.Inquiries.Select(inquiry => Map(inquiry, depth + 1)).ToList();
            }
            _active.Remove(x);
            return dto;
        }
    }
}
