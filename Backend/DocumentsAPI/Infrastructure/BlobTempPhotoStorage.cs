using Azure.Storage.Blobs.Models;

namespace DocumentsAPI.Infrastructure;

public class BlobTempPhotoStorage(
    BlobDbContext context) : ITempPhotoStorage
{
    public async Task UploadAsync(Guid userId, Stream stream, TimeSpan ttl, CancellationToken ct)
    {
        var containerClient = context.TempProfilePhotoContainerClient;
        var blobClient = containerClient.GetBlobClient($"{userId}.jpg");
        
        var expiresOn = DateTime.UtcNow.Add(ttl);
        var options = new BlobUploadOptions
        {
            Tags = new Dictionary<string, string>
            {
                { "expiresOn", expiresOn.Ticks.ToString() }
            }
        };

        await blobClient.UploadAsync(stream, options, ct);
    }
}

public interface ITempPhotoStorage
{
    public Task UploadAsync(Guid userId, Stream stream, TimeSpan ttl, CancellationToken ct);
}

public interface IProfilePhotoStorage
{
    Task<bool> DeletePhotoAsync(Guid userId, Guid photoId, CancellationToken ct);
    Task<bool> ConfirmPhotoAsync(Guid userId, Guid photoId, CancellationToken ct);
}

public class BlobProfilePhotoStorage(BlobDbContext context) : IProfilePhotoStorage
{
    public async Task<bool> DeletePhotoAsync(Guid userId, Guid photoId, CancellationToken ct)
    {
        var activeContainer = context.ActiveProfilePhotoContainerClient;
        var activeBlobClient = activeContainer.GetBlobClient($"{userId}/{photoId}.jpg");
        if (await activeBlobClient.ExistsAsync(ct)) 
        {
            await activeBlobClient.DeleteIfExistsAsync(cancellationToken: ct);
            return true; 
        }

        return false;
    }

    public async Task<bool> ConfirmPhotoAsync(Guid userId, Guid photoId, CancellationToken ct)
    {
        var tempContainer = context.TempProfilePhotoContainerClient;
        var tempBlobClient = tempContainer.GetBlobClient($"{userId}.jpg");
        
        var activeContainer = context.ActiveProfilePhotoContainerClient;
        var activeBlobClient = activeContainer.GetBlobClient($"{userId}/{photoId}.jpg");
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
}