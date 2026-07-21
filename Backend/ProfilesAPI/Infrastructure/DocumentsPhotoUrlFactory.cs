using ProfilesAPI.Application;

namespace ProfilesAPI.Infrastructure;

public class DocumentsPhotoUrlFactory(string gatewayBaseUrl) : IPhotoUrlFactory
{
    public string GenerateDoctorPhotoUrl(Guid doctorId, Guid photoId)
    {
        return $"{gatewayBaseUrl}/documents/api/v1/Photos/doctors/{doctorId}/avatar/{photoId}";
    }

    public string GenerateMeUserPhotoUrl(Guid photoId)
    {
        return $"{gatewayBaseUrl}/documents/api/v1/Photos/users/avatar/{photoId}";
    }
}