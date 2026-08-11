using ProfilesAPI.Application;
using IGatewayUrlProvider = ServiceDefaults.IGatewayUrlProvider;

namespace ProfilesAPI.Infrastructure;

public class DocumentsPhotoUrlFactory(IGatewayUrlProvider gatewayUrlProvider) : IPhotoUrlFactory
{
    
    public string GenerateDoctorPhotoUrl(Guid doctorId, Guid photoId)
    {
        return $"{gatewayUrlProvider.BaseUrl}/documents/api/v1/Photos/doctors/{doctorId}/avatar/{photoId}";
    }

    public string GenerateUserPhotoUrl(Guid userId, Guid photoId)
    {
        return $"{gatewayUrlProvider.BaseUrl}/documents/api/v1/Photos/users/{userId}/avatar/{photoId}";
    }

    public string GenerateMeUserPhotoUrl(Guid photoId)
    {
        return $"{gatewayUrlProvider.BaseUrl}/documents/api/v1/Photos/users/avatar/{photoId}";
    }
}

public class FrontendUrlGenerator(IGatewayUrlProvider gatewayUrlProvider)  : IFrontendUrlGenerator
{
    public string GenerateFrontendIndexUrl()
    {
        return gatewayUrlProvider.BaseUrl;
    }
}