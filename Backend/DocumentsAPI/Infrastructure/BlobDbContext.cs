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