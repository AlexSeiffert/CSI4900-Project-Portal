namespace API.DTOs;

public sealed record ProjectPageRow(Project Project, string SupervisorName, int InquiryCount, int SaveCount);
