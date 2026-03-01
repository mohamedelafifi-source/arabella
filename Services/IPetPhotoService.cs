namespace arabella.Services;

public sealed record PetPhotoUploadResult(string? Url, string? ErrorMessage);

public interface IPetPhotoService
{
    Task<PetPhotoUploadResult> UploadAsync(string unitNumber, int petId, Stream content, string contentType, CancellationToken ct = default);
    Task DeleteAsync(string? photoUrl, CancellationToken ct = default);
    /// <summary>Downloads a blob by URL; returns (stream, contentType) or null if not configured or blob not found.</summary>
    Task<(Stream Stream, string ContentType)?> TryDownloadAsync(string? photoUrl, CancellationToken ct = default);
}
