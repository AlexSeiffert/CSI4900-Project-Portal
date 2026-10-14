namespace API.DTOs;

public class Topic : BaseDto<Guid>
{
    public string Name { get; set; } = string.Empty;
}
