using Azure;
using Azure.Storage.Blobs;

namespace DocumentsAPI.Infrastructure.Photos;

public interface IUserPhotoCleaner
{
    public Task DeleteAllUserPhotos(string userId, CancellationToken ct);
}
public interface IPhotoRepository
{
    public BlobClient GetPhotoClient(string userId, Guid id);
    public BlobClient GetTempPhotoClient(string userId, Guid id);
}

public class ProfilePhotoRepository(BlobDbContext context) : IPhotoRepository, IUserPhotoCleaner
{
    private string GetPhotoName(string userId, Guid id) => $"{userId}/{id}.jpg";

    public async Task DeleteAllUserPhotos(string userId, CancellationToken ct)
    {
        var activeContainer = context.ActiveProfilePhotoContainerClient;
        var tempContainer = context.TempProfilePhotoContainerClient;

        await BlobContainerHelper.DeleteBlobsByPrefixAsync(activeContainer, userId, ct);
        await BlobContainerHelper.DeleteBlobsByPrefixAsync(tempContainer, userId, ct);
    }
    
    public BlobClient GetPhotoClient(string userId, Guid id)
        => context.ActiveProfilePhotoContainerClient.GetBlobClient(GetPhotoName(userId,id));
    
    public BlobClient GetTempPhotoClient(string userId, Guid id)
        => context.TempProfilePhotoContainerClient.GetBlobClient(GetPhotoName(userId, id));
}