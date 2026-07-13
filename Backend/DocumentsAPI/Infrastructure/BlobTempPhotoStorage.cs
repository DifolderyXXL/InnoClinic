using Azure.Storage.Blobs.Models;

namespace DocumentsAPI.Infrastructure;

public class BlobTempPhotoStorage(
    PhotoRepository context) : ITempPhotoStorage
{
    public async Task<Guid> UploadAsync(Guid userId, Stream stream, TimeSpan ttl, CancellationToken ct)
    {
        var photoId = Guid.NewGuid();
        var blobClient = context.GetTempProfilePhotoClient(userId, photoId);
        
        var expiresOn = DateTime.UtcNow.Add(ttl);
        var options = new BlobUploadOptions
        {
            Tags = new Dictionary<string, string>
            {
                { "expiresOn", expiresOn.Ticks.ToString() }
            }
        };

        await blobClient.UploadAsync(stream, options, ct);

        return photoId;
    }
}

public interface ITempPhotoStorage
{
    public Task<Guid> UploadAsync(Guid userId, Stream stream, TimeSpan ttl, CancellationToken ct);
}

