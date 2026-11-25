namespace API.Services;

// Who is making the request, from their login token.
public readonly record struct Caller(Guid Id, bool IsAdmin = false);
