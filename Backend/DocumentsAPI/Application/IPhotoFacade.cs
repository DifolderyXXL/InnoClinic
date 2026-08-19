namespace DocumentsAPI.Application;


public record DoctorPhotoResult(string Url, double ExpireTimeMillis);

public enum DoctorPhotoStatus
{
    Success,
    NotFound,
    Forbidden
}

public record DoctorPhotoResponse(DoctorPhotoStatus Status, DoctorPhotoResult? Result = null);

public interface IPhotoFacade
{
    public Task<bool> ConfirmOfficePhotoAsync(
        string officeId,
        Guid photoId,
        Guid? oldPhotoId,
        CancellationToken ct);
    
    public Task<Guid> UploadOfficePhoto(
        string officeId,
        Stream stream,
        CancellationToken ct);
    public Task<Guid> UploadProfilePhoto(
        Guid userId,
        Stream stream,
        CancellationToken ct);
    
    Task<string?> GetPublicPhoto(
        string officeId,
        Guid photoId,
        CancellationToken ct);

    Task<string?> GetProfilePhoto(
        Guid userId,
        Guid photoId,
        CancellationToken ct);

    Task<DoctorPhotoResponse> GetDoctorPhoto(
        Guid doctorId,
        Guid photoId,
        CancellationToken ct);

    public Task DeleteAllUserPhotos(
        Guid userId,
        CancellationToken ct);
}