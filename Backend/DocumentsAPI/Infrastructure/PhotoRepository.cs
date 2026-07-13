using Azure.Storage.Blobs;

namespace DocumentsAPI.Infrastructure;

public class PhotoRepository(BlobDbContext context)
{
    private string GetPhotoName(Guid userId, Guid id) => $"{userId}/{id}.jpg";

    
    public BlobClient GetProfilePhotoClient(Guid userId, Guid id)
        => context.ActiveProfilePhotoContainerClient.GetBlobClient(GetPhotoName(userId,id));
    
    public BlobClient GetTempProfilePhotoClient(Guid userId, Guid id)
        => context.TempProfilePhotoContainerClient.GetBlobClient(GetPhotoName(userId, id));
}