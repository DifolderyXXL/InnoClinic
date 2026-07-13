using Azure.Storage.Blobs;

namespace DocumentsAPI.Infrastructure;

public interface IPhotoRepository
{
    public BlobClient GetPhotoClient(string userId, Guid id);
    public BlobClient GetTempPhotoClient(string userId, Guid id);
}

public class ProfilePhotoRepository(BlobDbContext context) : IPhotoRepository
{
    private string GetPhotoName(string userId, Guid id) => $"{userId}/{id}.jpg";

    
    public BlobClient GetPhotoClient(string userId, Guid id)
        => context.ActiveProfilePhotoContainerClient.GetBlobClient(GetPhotoName(userId,id));
    
    public BlobClient GetTempPhotoClient(string userId, Guid id)
        => context.TempProfilePhotoContainerClient.GetBlobClient(GetPhotoName(userId, id));
}