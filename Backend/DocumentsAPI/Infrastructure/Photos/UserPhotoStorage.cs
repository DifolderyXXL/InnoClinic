namespace DocumentsAPI.Infrastructure.Photos;

public class UserPhotoStorage(ProfilePhotoRepository context) 
    : BlobPhotoStorage(context), IUserPhotoStorage;