namespace API.DTOs;

public abstract class AuditableDto<TId> : BaseDto<TId>
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
