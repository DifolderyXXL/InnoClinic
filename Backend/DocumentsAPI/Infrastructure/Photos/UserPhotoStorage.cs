namespace DocumentsAPI.Infrastructure;

public class UserPhotoStorage(ProfilePhotoRepository context) 
    : BlobPhotoStorage(context), IUserPhotoStorage;