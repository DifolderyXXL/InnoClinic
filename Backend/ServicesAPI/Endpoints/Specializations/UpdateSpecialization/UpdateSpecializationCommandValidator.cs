using FluentValidation;

namespace ServicesAPI.Endpoints.Specializations.UpdateSpecialization;

public class UpdateSpecializationCommandValidator : AbstractValidator<UpdateSpecializationCommand>
{
    public UpdateSpecializationCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.SpecializationName).NotEmpty().MaximumLength(100);
    }
}
