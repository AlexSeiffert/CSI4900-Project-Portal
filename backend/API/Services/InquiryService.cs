using API.DTOs.Mapping;
using API.DTOs.Request;
using API.Exceptions;
using API.Models;
using API.Repositories.Interfaces;
using API.Services.Interfaces;
using API.Storage;

namespace API.Services;

// The checks here mirror the database's rules (db/migrations/0009 and 0010) so that the API gives
// clear errors, and so the in-memory development database behaves the same. With Postgres, row level
// security is still what actually enforces them.
public class InquiryService(IInquiryRepository repository, IProjectRepository projects, IAttachmentStorage storage) : IInquiryService
{
    private const string PdfMimeType = "application/pdf";

    private readonly IInquiryRepository _repository = repository;
    private readonly IProjectRepository _projects = projects;
    private readonly IAttachmentStorage _storage = storage;

    public async Task<IEnumerable<DTOs.Inquiry>> GetInquiries(CancellationToken cancellationToken = default)
    {
        var inquiries = await _repository.GetInquiries(cancellationToken);
        return inquiries.Select(inquiry => inquiry.ToDto()).ToList();
    }

    public async Task<IEnumerable<DTOs.Inquiry>> GetInquiriesByStudentId(Guid studentId, CancellationToken cancellationToken = default)
    {
        var inquiries = await _repository.GetInquiriesByStudentId(studentId, cancellationToken);
        return inquiries.Select(inquiry => inquiry.ToDto()).ToList();
    }

    public async Task<IEnumerable<DTOs.Inquiry>> GetInquiriesBySupervisorId(Guid supervisorId, CancellationToken cancellationToken = default)
    {
        var inquiries = await _repository.GetInquiriesBySupervisorId(supervisorId, cancellationToken);
        return inquiries.Select(inquiry => inquiry.ToDto()).ToList();
    }

    public async Task<IEnumerable<DTOs.Inquiry>> GetInquiriesByProjectId(Guid projectId, CancellationToken cancellationToken = default)
    {
        var inquiries = await _repository.GetInquiriesByProjectId(projectId, cancellationToken);
        return inquiries.Select(inquiry => inquiry.ToDto()).ToList();
    }

    public async Task<DTOs.Inquiry> GetInquiryById(Guid inquiryId, Caller caller, CancellationToken cancellationToken = default)
    {
        var inquiry = await Require(inquiryId, caller, nameof(GetInquiryById), cancellationToken);
        return await ToDetail(inquiry, cancellationToken);
    }

    public async Task<DTOs.Inquiry> CreateInquiry(CreateInquiry request, CancellationToken cancellationToken = default)
    {
        var projectId = request.ProjectId ?? throw ServiceException.Validation(nameof(CreateInquiry), "Choose a project.");
        var project = await _projects.GetProjectById(projectId, cancellationToken)
            ?? throw ServiceException.Validation(nameof(CreateInquiry), $"Project '{projectId}' does not exist.");
        var message = string.IsNullOrWhiteSpace(request.Message) ? null : request.Message.Trim();
        if (message?.Length > DTOs.Request.CreateInquiry.MessageMaxLength)
        {
            throw ServiceException.Validation(nameof(CreateInquiry), $"The message can be at most {DTOs.Request.CreateInquiry.MessageMaxLength} characters.");
        }

        var inquiry = new Inquiry
        {
            Id = Guid.NewGuid(),
            StudentId = request.StudentId,
            SupervisorId = project.SupervisorId,
            ProjectId = project.Id,
            Message = message,
            Status = InquiryStatus.Draft,
            Members = [new InquiryMember { StudentId = request.StudentId, Response = InquiryResponse.Pending }]
        };
        return await ToDetail(await _repository.CreateInquiry(inquiry, cancellationToken), cancellationToken);
    }

    public async Task<DTOs.Inquiry> AddMember(Guid inquiryId, AddInquiryMember request, Caller caller, CancellationToken cancellationToken = default)
    {
        const string operation = nameof(AddMember);
        var inquiry = await Require(inquiryId, caller, operation, cancellationToken);
        if (inquiry.StudentId != caller.Id && !caller.IsAdmin)
        {
            throw ServiceException.Forbidden(operation, "Only the student who created the request can add members.");
        }
        if (inquiry.Status != InquiryStatus.Draft)
        {
            throw ServiceException.Validation(operation, "Members can only be added while the request is a draft.");
        }
        if (inquiry.Members.Count >= InquiryRules.MaxMembers)
        {
            throw ServiceException.Validation(operation, $"A request can have at most {InquiryRules.MaxMembers} students.");
        }
        var studentId = await _repository.FindStudentIdByEmail(request.Email, cancellationToken)
            ?? throw ServiceException.Validation(operation, "No student is registered with that email.");
        if (inquiry.Members.Any(member => member.StudentId == studentId))
        {
            throw ServiceException.Validation(operation, "That student is already on this request.");
        }

        await _repository.AddMember(new InquiryMember { InquiryId = inquiryId, StudentId = studentId }, cancellationToken);
        return await GetInquiryById(inquiryId, caller, cancellationToken);
    }

    // A member can leave a request before it is settled; the creator can remove a friend from a draft.
    public async Task RemoveMember(Guid inquiryId, Guid studentId, Caller caller, CancellationToken cancellationToken = default)
    {
        const string operation = nameof(RemoveMember);
        var inquiry = await Require(inquiryId, caller, operation, cancellationToken);
        if (inquiry.Members.All(member => member.StudentId != studentId))
        {
            throw ServiceException.NotFound(operation, "That student is not on this request.");
        }
        if (studentId == inquiry.StudentId)
        {
            throw ServiceException.Validation(operation, "The student who created the request cannot be removed from it.");
        }
        var leaving = studentId == caller.Id && inquiry.Status is InquiryStatus.Draft or InquiryStatus.Sent;
        var removing = (inquiry.StudentId == caller.Id || caller.IsAdmin) && inquiry.Status == InquiryStatus.Draft;
        if (!leaving && !removing)
        {
            throw ServiceException.Forbidden(operation, "You cannot remove that student from this request.");
        }
        await _repository.RemoveMember(inquiryId, studentId, cancellationToken);
    }

    public Task<DTOs.Inquiry> MarkSent(Guid inquiryId, Caller caller, CancellationToken cancellationToken = default)
    {
        return Move(inquiryId, caller, InquiryStatus.Sent, nameof(MarkSent), cancellationToken);
    }

    public Task<DTOs.Inquiry> Offer(Guid inquiryId, Caller caller, CancellationToken cancellationToken = default)
    {
        return Move(inquiryId, caller, InquiryStatus.Offered, nameof(Offer), cancellationToken);
    }

    public Task<DTOs.Inquiry> Close(Guid inquiryId, Caller caller, CancellationToken cancellationToken = default)
    {
        return Move(inquiryId, caller, InquiryStatus.Closed, nameof(Close), cancellationToken);
    }

    public async Task<DTOs.RespondResult> Respond(Guid inquiryId, bool accept, Caller caller, CancellationToken cancellationToken = default)
    {
        const string operation = nameof(Respond);
        var inquiry = await Require(inquiryId, caller, operation, cancellationToken);
        var member = inquiry.Members.SingleOrDefault(row => row.StudentId == caller.Id)
            ?? throw ServiceException.Forbidden(operation, "Only a member of the request can answer the offer.");
        if (inquiry.Status != InquiryStatus.Offered)
        {
            throw ServiceException.Validation(operation, "There is no open offer on this request.");
        }
        if (member.Response != InquiryResponse.Pending)
        {
            throw ServiceException.Validation(operation, "You have already answered this offer.");
        }

        var outcome = await _repository.Respond(inquiryId, caller.Id, accept, cancellationToken);
        return new DTOs.RespondResult(outcome.Status, outcome.DroppedInquiryIds);
    }

    public async Task<DTOs.SignedUrl> CreateUploadUrl(Guid inquiryId, CreateUploadUrl request, Caller caller, CancellationToken cancellationToken = default)
    {
        const string operation = nameof(CreateUploadUrl);
        var inquiry = await RequireMember(inquiryId, caller, operation, cancellationToken);
        if (inquiry.Status == InquiryStatus.Closed)
        {
            throw ServiceException.Validation(operation, "Files cannot be added to a closed request.");
        }
        var kind = request.Kind ?? throw ServiceException.Validation(operation, "Choose resume or transcript.");
        var path = $"{caller.Id}/{inquiryId}/{kind.ToString().ToLowerInvariant()}-{Guid.NewGuid():N}.pdf";
        return new DTOs.SignedUrl(await _storage.CreateUploadUrl(path, cancellationToken), path);
    }

    public async Task<DTOs.InquiryAttachment> RegisterAttachment(Guid inquiryId, RegisterAttachment request, Caller caller, CancellationToken cancellationToken = default)
    {
        const string operation = nameof(RegisterAttachment);
        var inquiry = await RequireMember(inquiryId, caller, operation, cancellationToken);
        if (inquiry.Status == InquiryStatus.Closed)
        {
            throw ServiceException.Validation(operation, "Files cannot be added to a closed request.");
        }
        if (!request.StoragePath.StartsWith($"{caller.Id}/{inquiryId}/", StringComparison.Ordinal))
        {
            throw ServiceException.Validation(operation, "Use the storage path returned with the upload URL.");
        }
        if (request.MimeType != PdfMimeType)
        {
            throw ServiceException.Validation(operation, "Attachments must be PDF files.");
        }
        if (request.SizeBytes is < 1 or > DTOs.Request.RegisterAttachment.MaxSizeBytes)
        {
            throw ServiceException.Validation(operation, "Attachments must be between 1 byte and 2 MB.");
        }

        var attachment = await _repository.AddAttachment(new InquiryAttachment
        {
            Id = Guid.NewGuid(),
            InquiryId = inquiryId,
            StudentId = caller.Id,
            Kind = request.Kind ?? throw ServiceException.Validation(operation, "Choose resume or transcript."),
            StoragePath = request.StoragePath,
            MimeType = request.MimeType,
            SizeBytes = request.SizeBytes
        }, cancellationToken);
        return attachment.ToDto();
    }

    public async Task<DTOs.SignedUrl> CreateDownloadUrl(Guid inquiryId, Guid attachmentId, Caller caller, CancellationToken cancellationToken = default)
    {
        var inquiry = await Require(inquiryId, caller, nameof(CreateDownloadUrl), cancellationToken);
        var attachment = inquiry.Attachments.SingleOrDefault(row => row.Id == attachmentId)
            ?? throw ServiceException.NotFound(nameof(CreateDownloadUrl), $"Attachment '{attachmentId}' was not found.");
        return new DTOs.SignedUrl(await _storage.CreateDownloadUrl(attachment.StoragePath, cancellationToken));
    }

    // draft -> sent by a member; sent -> offered and anything -> closed by the supervisor.
    // matched (and closed after an offer) only ever come from Respond.
    private async Task<DTOs.Inquiry> Move(Guid inquiryId, Caller caller, InquiryStatus to, string operation, CancellationToken cancellationToken)
    {
        var inquiry = await Require(inquiryId, caller, operation, cancellationToken);
        var isMember = inquiry.Members.Any(member => member.StudentId == caller.Id);
        var isSupervisor = inquiry.SupervisorId == caller.Id || caller.IsAdmin;
        var (allowed, from) = to switch
        {
            InquiryStatus.Sent => (isMember || caller.IsAdmin, inquiry.Status == InquiryStatus.Draft),
            InquiryStatus.Offered => (isSupervisor, inquiry.Status == InquiryStatus.Sent),
            InquiryStatus.Closed => (isSupervisor, inquiry.Status != InquiryStatus.Closed),
            _ => (false, false)
        };
        if (!allowed)
        {
            throw ServiceException.Forbidden(operation, $"You cannot move this request to {to}.");
        }
        if (!from)
        {
            throw ServiceException.Validation(operation, $"A {inquiry.Status.ToString().ToLowerInvariant()} request cannot be moved to {to.ToString().ToLowerInvariant()}.");
        }

        inquiry.Status = to;
        await _repository.SaveInquiry(inquiry, cancellationToken);
        return await ToDetail(inquiry, cancellationToken);
    }

    private async Task<Inquiry> Require(Guid inquiryId, Caller caller, string operation, CancellationToken cancellationToken)
    {
        var inquiry = await _repository.GetInquiryById(inquiryId, cancellationToken);
        if (inquiry == null || !CanSee(inquiry, caller))
        {
            throw ServiceException.NotFound(operation, $"Inquiry '{inquiryId}' was not found.");
        }
        return inquiry;
    }

    private async Task<Inquiry> RequireMember(Guid inquiryId, Caller caller, string operation, CancellationToken cancellationToken)
    {
        var inquiry = await Require(inquiryId, caller, operation, cancellationToken);
        if (inquiry.Members.All(member => member.StudentId != caller.Id))
        {
            throw ServiceException.Forbidden(operation, "Only a member of the request can add files to it.");
        }
        return inquiry;
    }

    private static bool CanSee(Inquiry inquiry, Caller caller)
    {
        return caller.IsAdmin || inquiry.SupervisorId == caller.Id || inquiry.StudentId == caller.Id
            || inquiry.Members.Any(member => member.StudentId == caller.Id);
    }

    private async Task<DTOs.Inquiry> ToDetail(Inquiry inquiry, CancellationToken cancellationToken)
    {
        var dto = inquiry.ToDtoWithRelations();
        var emails = await _repository.GetMemberEmails(inquiry.Id, cancellationToken);
        foreach (var member in dto.Members)
        {
            member.Email = emails.GetValueOrDefault(member.StudentId);
        }
        return dto;
    }
}
