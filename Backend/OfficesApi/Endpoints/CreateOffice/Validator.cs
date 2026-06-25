using FluentValidation;

namespace OfficesApi.Endpoints.CreateOffice;

public class CreateOfficeCommandValidator : AbstractValidator<CreateOfficeCommand>
{
    public CreateOfficeCommandValidator()
    {
        RuleFor(x => x.City).NotEmpty().MaximumLength(64);
        RuleFor(x => x.HouseNumber).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Street).NotEmpty().MaximumLength(64);
        RuleFor(x => x.RegistryPhoneNumber).NotEmpty().MaximumLength(64);
    }
}