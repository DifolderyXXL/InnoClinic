using Azure.Storage.Blobs.Models;

namespace DocumentsAPI.Infrastructure.Photos;

public interface IPhotoStorage
{
    public IPhotoRepository Repository { get; }
    Task<bool> DeletePhotoAsync(string userId, Guid photoId, CancellationToken ct);
    Task<bool> ConfirmPhotoAsync(string userId, Guid photoId, CancellationToken ct);
    
    public Task<Guid> UploadTempAsync(string userId, Stream stream, TimeSpan ttl, CancellationToken ct);
}

public class BlobPhotoStorage(IPhotoRepository context) : IPhotoStorage
{
    public IPhotoRepository Repository => context;
    public async Task<bool> DeletePhotoAsync(string userId, Guid photoId, CancellationToken ct)
    {
        var activeBlobClient = context.GetPhotoClient(userId, photoId);
        if (await activeBlobClient.ExistsAsync(ct)) 
        {
            await activeBlobClient.DeleteIfExistsAsync(cancellationToken: ct);
            return true; 
        }

        return false;
    }

    public async Task<bool> ConfirmPhotoAsync(string userId, Guid photoId, CancellationToken ct)
    {
        var tempBlobClient = context.GetTempPhotoClient(userId, photoId);
        
        var activeBlobClient = context.GetPhotoClient(userId, photoId);
        if (await activeBlobClient.ExistsAsync(ct)) 
        {
            await tempBlobClient.DeleteIfExistsAsync(cancellationToken: ct);
            return true; 
        }
        
        if (!await tempBlobClient.ExistsAsync(ct)) return false;
        
        var copyOperation = await activeBlobClient.StartCopyFromUriAsync(tempBlobClient.Uri, cancellationToken: ct);
        await copyOperation.WaitForCompletionAsync(ct);

        await tempBlobClient.DeleteIfExistsAsync(cancellationToken: ct);
        return true;
    }
    
    public async Task<Guid> UploadTempAsync(string userId, Stream stream, TimeSpan ttl, CancellationToken ct)
    {
        var photoId = Guid.NewGuid();
        var blobClient = context.GetTempPhotoClient(userId.ToString(), photoId);
        
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