namespace DocumentsAPI.Infrastructure;

public class PublicPhotoStorage(PublicPhotoRepository context) 
    : BlobPhotoStorage(context), IPublicPhotoStorage;