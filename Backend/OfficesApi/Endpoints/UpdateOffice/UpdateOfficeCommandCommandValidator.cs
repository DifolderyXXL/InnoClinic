using FluentValidation;

namespace OfficesApi.Endpoints.UpdateOffice;

public class UpdateOfficeCommandCommandValidator : AbstractValidator<UpdateOfficeCommand>
{
    public UpdateOfficeCommandCommandValidator()
    {
        RuleFor(x => x.OfficeId).NotEmpty();
        RuleFor(x => x.City).NotEmpty().MaximumLength(64);
        RuleFor(x => x.HouseNumber).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Street).NotEmpty().MaximumLength(64);
        RuleFor(x => x.RegistryPhoneNumber).NotEmpty().MaximumLength(64);
    }
}