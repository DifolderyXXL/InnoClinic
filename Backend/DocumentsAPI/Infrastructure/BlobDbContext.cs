using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace DocumentsAPI.Infrastructure;

public class BlobDbContext(BlobServiceClient client)
{
    public const string ActivePhotoContainerClientName = "photo-icons-active";
    public const string TempPhotoContainerClientName = "photo-icons-temp";

    public const string PublicActivePhotoContainerClientName = "public-photo-icons-active";
    public const string PublicTempPhotoContainerClientName = "public-photo-icons-temp";
    
    public const string MedicalResultsContainerClientName = "medical-results";
    
    public BlobContainerClient ActiveProfilePhotoContainerClient =>
        client.GetBlobContainerClient(ActivePhotoContainerClientName);
    
    public BlobContainerClient TempProfilePhotoContainerClient =>
        client.GetBlobContainerClient(TempPhotoContainerClientName);
    
    public BlobContainerClient PublicActiveProfilePhotoContainerClient =>
        client.GetBlobContainerClient(PublicActivePhotoContainerClientName);
    
    public BlobContainerClient PublicTempProfilePhotoContainerClient =>
        client.GetBlobContainerClient(PublicTempPhotoContainerClientName);
    
    public BlobContainerClient MedicalResultsContainerClient =>
        client.GetBlobContainerClient(MedicalResultsContainerClientName);
    
    public async Task EnsureCreated(CancellationToken ct)
    {
        await ActiveProfilePhotoContainerClient.CreateIfNotExistsAsync(cancellationToken: ct);
        await TempProfilePhotoContainerClient.CreateIfNotExistsAsync(cancellationToken: ct);
        
        await PublicActiveProfilePhotoContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: ct);
        await PublicTempProfilePhotoContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: ct);
        
        await MedicalResultsContainerClient.CreateIfNotExistsAsync(cancellationToken: ct);
    }
}