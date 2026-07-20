namespace DocumentsAPI.Infrastructure.Photos;

public class PublicPhotoStorage(PublicPhotoRepository context) 
    : BlobPhotoStorage(context), IPublicPhotoStorage;