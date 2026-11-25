namespace API.Storage;

public interface IAttachmentStorage
{
    Task<string> CreateUploadUrl(string storagePath, CancellationToken cancellationToken = default);
    Task<string> CreateDownloadUrl(string storagePath, CancellationToken cancellationToken = default);
}
