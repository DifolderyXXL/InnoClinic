using Azure.Storage.Blobs;

namespace DocumentsAPI.Infrastructure;

public class BlobDbContext(BlobServiceClient client)
{
    public const string ActivePhotoContainerClientName = "photo-icons-active";
    public const string TempPhotoContainerClientName = "photo-icons-temp";

    public BlobContainerClient ActiveProfilePhotoContainerClient =>
        client.GetBlobContainerClient(ActivePhotoContainerClientName);
    
    public BlobContainerClient TempProfilePhotoContainerClient =>
        client.GetBlobContainerClient(TempPhotoContainerClientName);

    public async Task EnsureCreated(CancellationToken ct)
    {
        await ActiveProfilePhotoContainerClient.CreateIfNotExistsAsync(cancellationToken: ct);
        await TempProfilePhotoContainerClient.CreateIfNotExistsAsync(cancellationToken: ct);
    }
}


public class PhotoRepository(BlobDbContext context)
{
    private string GetPhotoName(Guid userId, Guid id) => $"{userId}/{id}.jpg";
    private string GetPhotoName(Guid userId) => $"temp-{userId}.jpg";
    
    public BlobClient GetProfilePhotoClient(Guid userId, Guid id)
        => context.ActiveProfilePhotoContainerClient.GetBlobClient(GetPhotoName(userId,id));
    
    public BlobClient TempProfilePhotoContainerClient(Guid userId)
        => context.TempProfilePhotoContainerClient.GetBlobClient(GetPhotoName(userId));
}

