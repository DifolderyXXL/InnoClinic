using FluentValidation;

namespace ProfilesAPI.Endpoints.Doctors.UpdateDoctorProfile;

public class UpdateDoctorProfileCommandValidator : AbstractValidator<UpdateDoctorProfileCommand>
{
    public UpdateDoctorProfileCommandValidator()
    {

    }
}