namespace DocumentsAPI.Infrastructure;

public interface IProfilePhotoStorage
{
    Task<bool> DeletePhotoAsync(Guid userId, Guid photoId, CancellationToken ct);
    Task<bool> ConfirmPhotoAsync(Guid userId, Guid photoId, CancellationToken ct);
}

public class BlobProfilePhotoStorage(PhotoRepository context) : IProfilePhotoStorage
{
    public async Task<bool> DeletePhotoAsync(Guid userId, Guid photoId, CancellationToken ct)
    {
        var activeBlobClient = context.GetProfilePhotoClient(userId, photoId);
        if (await activeBlobClient.ExistsAsync(ct)) 
        {
            await activeBlobClient.DeleteIfExistsAsync(cancellationToken: ct);
            return true; 
        }

        return false;
    }

    public async Task<bool> ConfirmPhotoAsync(Guid userId, Guid photoId, CancellationToken ct)
    {
        var tempBlobClient = context.GetTempProfilePhotoClient(userId, photoId);
        
        var activeBlobClient = context.GetProfilePhotoClient(userId, photoId);
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