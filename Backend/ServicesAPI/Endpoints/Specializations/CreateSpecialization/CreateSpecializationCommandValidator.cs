using FluentValidation;

namespace ServicesAPI.Endpoints.Specializations.CreateSpecialization;

public class CreateSpecializationCommandValidator : AbstractValidator<CreateSpecializationCommand>
{
    public CreateSpecializationCommandValidator()
    {
        RuleFor(x => x.SpecializationName).NotEmpty().MaximumLength(100);
    }
}