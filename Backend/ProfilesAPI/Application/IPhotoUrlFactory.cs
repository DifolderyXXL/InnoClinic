namespace ProfilesAPI.Application;

public interface IGatewayUrlProvider
{
    public string BaseUrl { get; }
}

public interface IPhotoUrlFactory
{
    public string GenerateDoctorPhotoUrl(Guid doctorId, Guid photoId);
    public string GenerateUserPhotoUrl(Guid userId, Guid photoId);
    public string GenerateMeUserPhotoUrl(Guid photoId);
}

public interface IFrontendUrlGenerator
{
    public string GenerateFrontendIndexUrl();
}