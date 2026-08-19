using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using DocumentsAPI.Application;
using DocumentsAPI.Infrastructure.Photos;

namespace DocumentsAPI.Infrastructure;

public class PhotoFacade(IUserPhotoStorage userStorage, IPublicPhotoStorage publicStorage, IUserPhotoCleaner cleaner) : IPhotoFacade
{
    public async Task<bool> ConfirmOfficePhotoAsync(
        string officeId,
        Guid photoId,
        Guid? oldPhotoId,
        CancellationToken ct)
    {
        if (oldPhotoId.HasValue)
        {
            await publicStorage.DeletePhotoAsync(officeId, oldPhotoId.Value, ct);
        }

        return await publicStorage.ConfirmPhotoAsync(officeId, photoId, ct);
    }
    
    public async Task<Guid> UploadOfficePhoto(
        string officeId,
        Stream stream,
        CancellationToken ct)
    {
        var guid = await publicStorage.UploadTempAsync(officeId, stream, TimeSpan.FromHours(1), ct);
        
        return guid;
    }
    
    public async Task<Guid> UploadProfilePhoto(
        Guid userId,
        Stream stream,
        CancellationToken ct)
    {
        var guid = await userStorage.UploadTempAsync(userId.ToString(), stream, TimeSpan.FromHours(1), ct);
        
        return guid;
    }
    
    
    public async Task<string?> GetPublicPhoto(
        string officeId,
        Guid photoId,
        CancellationToken ct)
    {
        var client = publicStorage.Repository.GetPhotoClient(officeId, photoId);

        if (!await client.ExistsAsync(ct)) return null;

        return client.Uri.ToString();
    }
    
    public async Task<string?> GetProfilePhoto(
        Guid userId,
        Guid photoId,
        CancellationToken ct)
    {
        var client = userStorage.Repository.GetPhotoClient(userId.ToString(), photoId);

        if (!await client.ExistsAsync(ct)) return null;

        return GenerateSensitiveSasUri(client).ToString();
    }
    
    public async Task DeleteAllUserPhotos(
        Guid userId,
        CancellationToken ct)
    {
        await cleaner.DeleteAllUserPhotos(userId.ToString(), ct);
    }
    
    private Uri GenerateSensitiveSasUri(BlobClient client)
    {
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = client.BlobContainerName,
            BlobName = client.Name,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddSeconds(CacheHelper.SensitiveCacheTime)
        };
        sasBuilder.SetPermissions(BlobAccountSasPermissions.Read);

        return client.GenerateSasUri(sasBuilder);
    }
    
    public async Task<DoctorPhotoResponse> GetDoctorPhoto(
        Guid doctorId,
        Guid photoId,
        CancellationToken ct)
    {
        var client = userStorage.Repository.GetPhotoClient(doctorId.ToString(), photoId);

        if (!await client.ExistsAsync(ct)) return new(DoctorPhotoStatus.NotFound);
        if (!await IsPhotoPublic(client, ct)) return new(DoctorPhotoStatus.Forbidden);

        var expireTime = TimeSpan.FromSeconds(CacheHelper.PublicCacheTime);
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = client.BlobContainerName,
            BlobName = client.Name,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.Add(expireTime)
        };
        sasBuilder.SetPermissions(BlobAccountSasPermissions.Read);

        var sasUri = client.GenerateSasUri(sasBuilder);

        return new(DoctorPhotoStatus.Success, new(sasUri.ToString(), expireTime.TotalMilliseconds));
    }
    
    private async Task<bool> IsPhotoPublic(BlobClient blobClient, CancellationToken ct)
    {
        var tagsResponse = await blobClient.GetTagsAsync(cancellationToken: ct);
        var tags = tagsResponse.Value.Tags;

        return tags.TryGetValue("public", out var value) && value.Equals("true", StringComparison.OrdinalIgnoreCase);
    }
}