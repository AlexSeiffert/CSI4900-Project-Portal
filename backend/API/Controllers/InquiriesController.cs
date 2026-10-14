using API.DTOs;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using API.DTOs.Request;

namespace API.Controllers;

[ApiController]
[Route("api/v1/inquiries")]
public class InquiriesController(IInquiryService service) : ControllerBase
{
    private readonly IInquiryService _service = service;

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
        return await _service.GetInquiryById(inquiryId, cancellationToken);
    }

    [HttpPost]
    [ProducesResponseType<Inquiry>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Inquiry>> CreateInquiry(CreateInquiry request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateInquiry(request, cancellationToken);
        return CreatedAtAction(nameof(GetInquiryById), new { inquiryId = result.Id }, result);
    }

    [HttpPut("{inquiryId:guid}")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateInquiry(Guid inquiryId, UpdateInquiry request, CancellationToken cancellationToken)
    {
        await _service.UpdateInquiry(inquiryId, request, cancellationToken);
        return NoContent();
    }
}
