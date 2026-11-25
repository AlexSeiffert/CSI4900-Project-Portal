using Microsoft.AspNetCore.Components.Forms;
using uoProjects.Models;

namespace uoProjects.Services;

public class InquiriesService(HttpClient httpClient, NotificationService notifications) : HttpBaseService(httpClient, notifications)
{
    public const long MaxFileBytes = 2 * 1024 * 1024;

    // Uploads go straight to Supabase Storage through a signed URL, not through the API client.
    private static readonly HttpClient Storage = new() { Timeout = TimeSpan.FromSeconds(60) };

    public Task<List<Inquiry>?> ListAsync()
    {
        return GetAsync<List<Inquiry>>("api/v1/inquiries", "Could not load inquiries.");
    }

    public Task<List<Inquiry>?> ListForStudentAsync(Guid studentId)
    {
        return GetAsync<List<Inquiry>>($"api/v1/inquiries/students/{studentId}", "Could not load your requests.");
    }

    public Task<List<Inquiry>?> ListForSupervisorAsync(Guid supervisorId)
    {
        return GetAsync<List<Inquiry>>($"api/v1/inquiries/supervisors/{supervisorId}", "Could not load your inquiries.");
    }

    public Task<Inquiry?> GetAsync(Guid id)
    {
        return GetAsync<Inquiry>($"api/v1/inquiries/{id}", "Could not load the request.");
    }

    public Task<Inquiry?> CreateAsync(Guid projectId, string? message)
    {
        return SendAsync<Inquiry>(HttpMethod.Post, "api/v1/inquiries", new { projectId, message }, "Could not create the request.");
    }

    public Task<Inquiry?> AddMemberAsync(Guid id, string email)
    {
        return SendAsync<Inquiry>(HttpMethod.Post, $"api/v1/inquiries/{id}/members", new { email }, "Could not add that student.");
    }

    public Task<bool> RemoveMemberAsync(Guid id, Guid studentId)
    {
        return SendAsync(HttpMethod.Delete, $"api/v1/inquiries/{id}/members/{studentId}", "Could not remove that student.");
    }

    public Task<Inquiry?> MarkSentAsync(Guid id)
    {
        return SendAsync<Inquiry>(HttpMethod.Post, $"api/v1/inquiries/{id}/send", null, "Could not mark the request as sent.");
    }

    public Task<Inquiry?> OfferAsync(Guid id)
    {
        return SendAsync<Inquiry>(HttpMethod.Post, $"api/v1/inquiries/{id}/offer", null, "Could not offer a match.");
    }

    public Task<Inquiry?> CloseAsync(Guid id)
    {
        return SendAsync<Inquiry>(HttpMethod.Post, $"api/v1/inquiries/{id}/close", null, "Could not close the request.");
    }

    public Task<RespondResult?> RespondAsync(Guid id, bool accept)
    {
        return SendAsync<RespondResult>(HttpMethod.Post, $"api/v1/inquiries/{id}/respond", new { accept }, "Could not save your answer.");
    }

    public Task<SignedUrl?> GetAttachmentUrlAsync(Guid id, Guid attachmentId)
    {
        return GetAsync<SignedUrl>($"api/v1/inquiries/{id}/attachments/{attachmentId}/url", "Could not open the file.");
    }

    // Null if the file was refused or any step failed (each failure is reported).
    public async Task<InquiryAttachment?> UploadAsync(Guid id, AttachmentKind kind, IBrowserFile file, NotificationService notifications)
    {
        if (file.ContentType != "application/pdf" || file.Size is 0 or > MaxFileBytes)
        {
            notifications.Error("Choose a PDF of at most 2 MB.");
            return null;
        }
        var signed = await SendAsync<SignedUrl>(HttpMethod.Post, $"api/v1/inquiries/{id}/attachments/upload-url", new { kind }, "Could not start the upload.");
        if (signed?.StoragePath is null)
        {
            return null;
        }
        try
        {
            await using var stream = file.OpenReadStream(MaxFileBytes);
            using var content = new StreamContent(stream);
            content.Headers.ContentType = new("application/pdf");
            using var response = await Storage.PutAsync(signed.Url, content);
            if (!response.IsSuccessStatusCode)
            {
                notifications.Error("The file could not be uploaded.");
                return null;
            }
        }
        catch (Exception exception) when (exception is HttpRequestException or IOException or TaskCanceledException)
        {
            notifications.Error("The file could not be uploaded.");
            return null;
        }
        return await SendAsync<InquiryAttachment>(HttpMethod.Post, $"api/v1/inquiries/{id}/attachments",
            new { storagePath = signed.StoragePath, kind, mimeType = "application/pdf", sizeBytes = file.Size }, "Could not save the file.");
    }
}
