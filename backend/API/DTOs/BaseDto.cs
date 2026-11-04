namespace API.DTOs;

public abstract class BaseDto<TId>
{
    public TId Id { get; set; } = default!;
}
