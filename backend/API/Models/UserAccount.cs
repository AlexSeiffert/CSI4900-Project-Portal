namespace API.Models;

// public.users. Row level security only lets a user read their own row, so with Postgres the
// API looks up other users' emails through SECURITY DEFINER functions instead (see migration 0009).
public class UserAccount
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;
}
