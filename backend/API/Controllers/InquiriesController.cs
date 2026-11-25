using API.Auth;
using API.DTOs;
using API.Services;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using API.DTOs.Request;

namespace API.Controllers;

[ApiController]
[Route("api/v1/inquiries")]
public class InquiriesController(IInquiryService service) : ControllerBase
{
    private readonly IInquiryService _service = service;

    private Caller Caller => new(User.GetRequiredUserId(), User.IsAdmin());

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Inquiry>>> GetInquiries(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetInquiries(cancellationToken));
    }

    [HttpGet("students/{studentId:guid}")]
    public async Task<ActionResult<IEnumerable<Inquiry>>> GetInquiriesByStudentId(Guid studentId, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetInquiriesByStudentId(studentId, cancellationToken));
    }

    [HttpGet("supervisors/{supervisorId:guid}")]
    public async Task<ActionResult<IEnumerable<Inquiry>>> GetInquiriesBySupervisorId(Guid supervisorId, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetInquiriesBySupervisorId(supervisorId, cancellationToken));
    }

    [HttpGet("projects/{projectId:guid}")]
    public async Task<ActionResult<IEnumerable<Inquiry>>> GetInquiriesByProjectId(Guid projectId, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetInquiriesByProjectId(projectId, cancellationToken));
    }

    [HttpGet("{inquiryId:guid}")]
    [ProducesResponseType<Inquiry>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Inquiry>> GetInquiryById(Guid inquiryId, CancellationToken cancellationToken)
    {
        return await _service.GetInquiryById(inquiryId, Caller, cancellationToken);
    }

    [HttpPost]
    [ProducesResponseType<Inquiry>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Inquiry>> CreateInquiry(CreateInquiry request, CancellationToken cancellationToken)
    {
        // the inquiry's creator is taken from the login token, not from anything the client types in
        request.StudentId = User.GetRequiredUserId();
        var result = await _service.CreateInquiry(request, cancellationToken);
        return CreatedAtAction(nameof(GetInquiryById), new { inquiryId = result.Id }, result);
    }

    [HttpPost("{inquiryId:guid}/members")]
    [ProducesResponseType<Inquiry>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Inquiry>> AddMember(Guid inquiryId, AddInquiryMember request, CancellationToken cancellationToken)
    {
        return await _service.AddMember(inquiryId, request, Caller, cancellationToken);
    }

    [HttpDelete("{inquiryId:guid}/members/{studentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveMember(Guid inquiryId, Guid studentId, CancellationToken cancellationToken)
    {
        await _service.RemoveMember(inquiryId, studentId, Caller, cancellationToken);
        return NoContent();
    }

    [HttpPost("{inquiryId:guid}/send")]
    [ProducesResponseType<Inquiry>(StatusCodes.Status200OK)]
    public async Task<ActionResult<Inquiry>> MarkSent(Guid inquiryId, CancellationToken cancellationToken)
    {
        return await _service.MarkSent(inquiryId, Caller, cancellationToken);
    }

    [HttpPost("{inquiryId:guid}/offer")]
    [ProducesResponseType<Inquiry>(StatusCodes.Status200OK)]
    public async Task<ActionResult<Inquiry>> Offer(Guid inquiryId, CancellationToken cancellationToken)
    {
        return await _service.Offer(inquiryId, Caller, cancellationToken);
    }

    [HttpPost("{inquiryId:guid}/close")]
    [ProducesResponseType<Inquiry>(StatusCodes.Status200OK)]
    public async Task<ActionResult<Inquiry>> Close(Guid inquiryId, CancellationToken cancellationToken)
    {
        return await _service.Close(inquiryId, Caller, cancellationToken);
    }

    [HttpPost("{inquiryId:guid}/respond")]
    [ProducesResponseType<RespondResult>(StatusCodes.Status200OK)]
    public async Task<ActionResult<RespondResult>> Respond(Guid inquiryId, RespondToInquiry request, CancellationToken cancellationToken)
    {
        return await _service.Respond(inquiryId, request.Accept!.Value, Caller, cancellationToken);
    }

    [HttpPost("{inquiryId:guid}/attachments/upload-url")]
    [ProducesResponseType<SignedUrl>(StatusCodes.Status200OK)]
    public async Task<ActionResult<SignedUrl>> CreateUploadUrl(Guid inquiryId, CreateUploadUrl request, CancellationToken cancellationToken)
    {
        return await _service.CreateUploadUrl(inquiryId, request, Caller, cancellationToken);
    }

    [HttpPost("{inquiryId:guid}/attachments")]
    [ProducesResponseType<InquiryAttachment>(StatusCodes.Status201Created)]
    public async Task<ActionResult<InquiryAttachment>> RegisterAttachment(Guid inquiryId, RegisterAttachment request, CancellationToken cancellationToken)
    {
        var result = await _service.RegisterAttachment(inquiryId, request, Caller, cancellationToken);
        return CreatedAtAction(nameof(GetInquiryById), new { inquiryId }, result);
    }

    [HttpGet("{inquiryId:guid}/attachments/{attachmentId:guid}/url")]
    [ProducesResponseType<SignedUrl>(StatusCodes.Status200OK)]
    public async Task<ActionResult<SignedUrl>> CreateDownloadUrl(Guid inquiryId, Guid attachmentId, CancellationToken cancellationToken)
    {
        return await _service.CreateDownloadUrl(inquiryId, attachmentId, Caller, cancellationToken);
    }
}
