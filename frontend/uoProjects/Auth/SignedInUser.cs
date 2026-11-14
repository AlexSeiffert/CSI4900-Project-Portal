namespace uoProjects.Auth;

public sealed record SignedInUser(Guid Id, string Email, string Role, string Name, string AccessToken, DateTimeOffset ExpiresAt);
