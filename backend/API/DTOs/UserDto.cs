namespace API.DTOs;

public abstract class UserDto
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
}
