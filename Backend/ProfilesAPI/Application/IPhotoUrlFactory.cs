namespace ProfilesAPI.Application;

public interface IPhotoUrlFactory
{
    public string GenerateDoctorPhotoUrl(Guid doctorId, Guid photoId);
    public string GenerateMeUserPhotoUrl(Guid photoId);
}