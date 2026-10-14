using API.DTOs;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using API.DTOs.Request;

namespace API.Controllers;

[ApiController]
[Route("api/v1/students")]
public class StudentsController(IStudentService service) : ControllerBase
{
    private readonly IStudentService _service = service;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Student>>> GetStudents(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetStudents(cancellationToken));
    }

    [HttpGet("{userId:guid}")]
    [ProducesResponseType<Student>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Student>> GetStudentById(Guid userId, CancellationToken cancellationToken)
    {
        return await _service.GetStudentById(userId, cancellationToken);
    }

    [HttpPut("{userId:guid}")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStudent(Guid userId, UpdateStudent request, CancellationToken cancellationToken)
    {
        await _service.UpdateStudent(userId, request, cancellationToken);
        return NoContent();
    }
}
