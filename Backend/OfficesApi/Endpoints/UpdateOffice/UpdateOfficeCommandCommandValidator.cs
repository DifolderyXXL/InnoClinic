using FluentValidation;

namespace OfficesApi.Endpoints.UpdateOffice;

public class UpdateOfficeCommandCommandValidator : AbstractValidator<UpdateOfficeCommand>
{
    public UpdateOfficeCommandCommandValidator()
    {
        RuleFor(x => x.OfficeId).NotEmpty();
        
        RuleFor(x => x.City).MaximumLength(64);
        RuleFor(x => x.HouseNumber).MaximumLength(64);
        RuleFor(x => x.Street).MaximumLength(64);
        RuleFor(x => x.RegistryPhoneNumber).MaximumLength(64);
    }
}