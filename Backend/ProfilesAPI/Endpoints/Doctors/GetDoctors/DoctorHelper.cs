using Mapster;
using ProfilesAPI.Models;

namespace ProfilesAPI.Endpoints.Doctors.GetDoctors;

public static class DoctorHelper
{
    public static readonly TypeAdapterConfig Config = CreateMapsterConfig();

    private static TypeAdapterConfig CreateMapsterConfig()
    {
        var config = new TypeAdapterConfig();
    
        config.NewConfig<Doctor, DoctorDto>()
            .Map(dest => dest.SpecializationName, src => src.Specialization.SpecializationName);
        
        return config;
    }
}