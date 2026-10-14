using API.DTOs.Request;
using API.Exceptions;
using API.Orchestrations.Interfaces;
using API.Services.Interfaces;

namespace API.Orchestrations;

public class InquiryOrchestration(IInquiryService inquiries, IStudentService students, ISupervisorService supervisors, IProjectService projects) : IInquiryOrchestration
{
    private readonly IInquiryService _inquiries = inquiries;
    private readonly IStudentService _students = students;
    private readonly ISupervisorService _supervisors = supervisors;
    private readonly IProjectService _projects = projects;

    public async Task<DTOs.Inquiry> CreateInquiry(CreateInquiry request, CancellationToken cancellationToken = default)
    {
        try
        {
            await _students.ValidateStudentExists(request.StudentId, cancellationToken);
            await _supervisors.ValidateSupervisorExists(request.SupervisorId, cancellationToken);
            await _projects.ValidateProjectSupervisor(request.ProjectId, request.SupervisorId, cancellationToken);
            return await _inquiries.CreateInquiry(request, cancellationToken);
        }
        catch (ServiceException exception)
        {
            throw new OrchestrationException(nameof(CreateInquiry), exception);
        }
        catch (OperationCanceledException exception)
        {
            throw new OrchestrationException(nameof(CreateInquiry), ErrorType.Cancelled, "The operation was cancelled.", exception);
        }
        catch (Exception exception) when (exception is not OrchestrationException)
        {
            throw new OrchestrationException(nameof(CreateInquiry), ErrorType.Unexpected, "The operation could not be completed.", exception);
        }
    }
}
