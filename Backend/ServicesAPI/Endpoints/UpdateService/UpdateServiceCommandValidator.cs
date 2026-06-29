using FluentValidation;

namespace ServicesAPI.Endpoints.UpdateService;

public class UpdateServiceCommandValidator : AbstractValidator<UpdateServiceCommand>
{
    public UpdateServiceCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.ServiceName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).NotEmpty();
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.SpecializationId).GreaterThan(0);
    }
}
