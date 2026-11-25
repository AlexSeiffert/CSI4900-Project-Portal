namespace API.DTOs;

// StoragePath is set for an upload URL: send it back when registering the attachment.
public record SignedUrl(string Url, string? StoragePath = null);
