using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace arabella.Services;

public class PetPhotoService : IPetPhotoService
{
    private readonly string _connectionString;
    private readonly string _containerName;
    private readonly ILogger<PetPhotoService> _logger;

    public PetPhotoService(IConfiguration configuration, ILogger<PetPhotoService> logger)
    {
        _connectionString = configuration["AzureStorage:ConnectionString"] ?? "";
        _containerName = configuration["AzureStorage:ContainerName"] ?? "pet-photos";
        _logger = logger;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_connectionString);

    public async Task<PetPhotoUploadResult> UploadAsync(string unitNumber, int petId, Stream content, string contentType, CancellationToken ct = default)
    {
        if (!IsConfigured)
        {
            _logger.LogWarning("Pet photo upload skipped: Azure Storage not configured (AzureStorage:ConnectionString missing or empty).");
            return new PetPhotoUploadResult(null, "Azure Storage not configured (AzureStorage:ConnectionString missing or empty).");
        }
        if (string.IsNullOrWhiteSpace(contentType)) contentType = "image/jpeg";

        try
        {
            // Copy to new stream in case the original was already read or doesn't support seek
            using var ms = new MemoryStream();
            await content.CopyToAsync(ms, ct);
            ms.Position = 0;

            var container = new BlobContainerClient(_connectionString, _containerName);
            await container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: ct);
            var client = container.GetBlobClient($"{unitNumber}/{petId}{GetExtension(contentType)}");
            await client.UploadAsync(ms, new BlobHttpHeaders { ContentType = contentType }, cancellationToken: ct);
            return new PetPhotoUploadResult(client.Uri.ToString(), null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pet photo upload failed. Unit={UnitNumber}, PetId={PetId}. Check AzureStorage:ConnectionString and ContainerName.", unitNumber, petId);
            return new PetPhotoUploadResult(null, ex.Message);
        }
    }

    public async Task DeleteAsync(string? photoUrl, CancellationToken ct = default)
    {
        if (!IsConfigured || string.IsNullOrWhiteSpace(photoUrl)) return;

        try
        {
            var uri = new Uri(photoUrl);
            var path = uri.AbsolutePath.TrimStart('/');
            var slash = path.IndexOf('/');
            var container = slash > 0 ? path[..slash] : _containerName;
            var blobName = slash > 0 ? path[(slash + 1)..] : path;
            var client = new BlobClient(_connectionString, container, blobName);
            await client.DeleteIfExistsAsync(cancellationToken: ct);
        }
        catch { /* ignore */ }
    }

    public async Task<(Stream Stream, string ContentType)?> TryDownloadAsync(string? photoUrl, CancellationToken ct = default)
    {
        if (!IsConfigured || string.IsNullOrWhiteSpace(photoUrl)) return null;
        try
        {
            var uri = new Uri(photoUrl);
            var path = uri.AbsolutePath.TrimStart('/');
            var slash = path.IndexOf('/');
            var container = slash > 0 ? path[..slash] : _containerName;
            var blobName = slash > 0 ? path[(slash + 1)..] : path;
            var client = new BlobClient(_connectionString, container, blobName);
            var response = await client.DownloadStreamingAsync(cancellationToken: ct);
            if (response?.Value == null) return null;
            var contentType = GetContentTypeFromPath(photoUrl);
            return (response.Value.Content, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Pet photo download failed for URL (first 50 chars): {Url}", photoUrl?.Length > 50 ? photoUrl[..50] + "..." : photoUrl);
            return null;
        }
    }

    private static string GetExtension(string contentType)
    {
        return contentType switch
        {
            "image/jpeg" or "image/jpg" => ".jpg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            "image/webp" => ".webp",
            _ => ".jpg"
        };
    }

    private static string GetContentTypeFromPath(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return "image/jpeg";
        var path = new Uri(url, UriKind.RelativeOrAbsolute).AbsolutePath;
        return path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? "image/png"
            : path.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ? "image/gif"
            : path.EndsWith(".webp", StringComparison.OrdinalIgnoreCase) ? "image/webp"
            : "image/jpeg";
    }
}
